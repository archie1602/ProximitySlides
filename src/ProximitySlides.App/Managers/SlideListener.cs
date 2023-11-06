using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using ConcurrentCollections;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Models;
using ProximitySlides.Core.Extensions;

namespace ProximitySlides.App.Managers;

public class SlideListener : ISlideListener
{
    private readonly ILogger<SlideListener> _logger;
    private readonly IProximityListener _proximityListener;
    private readonly ICollection<BlePackageMessage> _speakerSlides;
    private readonly ConcurrentQueue<BlePackageMessage> _handlersQueue;
    
    private Func<SlideDto, Task>? _onListenResultHandler;
    private Action<ListenFailed>? _onListenFailedHandler;
    
    private Task? _queueWorkerTask;
    private CancellationTokenSource _queueWorkerCts;
    
    public SlideListener(
        ILogger<SlideListener> logger,
        IProximityListener proximityListener)
    {
        _logger = logger;
        _proximityListener = proximityListener;
        // TODO: add equality comparator
        _speakerSlides = new ConcurrentHashSet<BlePackageMessage>(new BlePackageEqualityComparer());
        _handlersQueue = new ConcurrentQueue<BlePackageMessage>();

        _queueWorkerCts = new CancellationTokenSource();
    }

    private void OnScanResult(BlePackageMessage package)
    {
        _handlersQueue.Enqueue(package);
    }

    private void OnScanFailed(ListenFailed errorCode)
    {
        try
        {
            _onListenFailedHandler?.Invoke(errorCode);

        }
        catch (Exception e)
        {
            // TODO: add log
        }
    }
    
    private async Task HandleMessage()
    {
        while (!_queueWorkerCts.Token.IsCancellationRequested)
        {
            if (!_handlersQueue.TryDequeue(out var package))
            {
                // TODO: move to config
                // await Task.Delay(TimeSpan.FromMilliseconds(100));
                continue;
            }
            
            try
            {
                var tmpPackage = _speakerSlides
                    .FirstOrDefault(it => it.CurrentPage == package.CurrentPage);

                if (tmpPackage is not null)
                {
                    TryUpdateExistingPackage(package, tmpPackage);
                    continue;
                }
            
                _speakerSlides.Add(package);
            
                var isAllTotalPagesEqual = _speakerSlides
                    .Select(it => it.TotalPages)
                    .Distinct()
                    .Count() == 1;

                if (!isAllTotalPagesEqual)
                {
                    // TODO: skip all and start again...
                    _speakerSlides.Clear();
                    _speakerSlides.Add(package);
                    continue;
                }

                if (_speakerSlides.Count != package.TotalPages)
                {
                    continue;
                }
            
                var slideMsg = TryDeserializeSlideMessage();

                if (slideMsg is null || !Uri.IsWellFormedUriString(slideMsg.Url, UriKind.Absolute))
                {
                    // TODO: add log
                    continue;
                }
            
                await InvokeHandler(slideMsg);
            }
            catch (Exception e)
            {
                // TODO: add log
            
                // TODO: clear нужно делать в случае, если упали на десериализации
                _speakerSlides.Clear();
            }
        }
    }
    
    private SlideMessage? TryDeserializeSlideMessage()
    {
        var payloads = _speakerSlides
            .Select(it => it.Payload)
            .ToList();

        var payloadBytes = ConcatArrays(payloads);
        var payloadStr = Encoding.ASCII.GetString(payloadBytes);
        var decompressSlideJson = payloadStr.DecompressJson();
        var slideMsg = JsonSerializer.Deserialize<SlideMessage>(decompressSlideJson);
        
        return slideMsg;
    }

    private void TryUpdateExistingPackage(BlePackageMessage package, BlePackageMessage tmpPackage)
    {
        if (tmpPackage.TotalPages == package.TotalPages &&
            tmpPackage.Payload.SequenceEqual(package.Payload))
        {
            // TODO: update timestamp and etc...
            tmpPackage.ReceivedAt = package.ReceivedAt;
            return;
        }

        // TODO: skip all and start again...
        _speakerSlides.Clear();
        _speakerSlides.Add(package);
    }
    
    private async Task InvokeHandler(SlideMessage slideMsg)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var t = _onListenResultHandler?.Invoke(new SlideDto
            {
                Url = new Uri(slideMsg.Url),
                CurrentSlide = slideMsg.CurrentSlide,
                TotalSlides = slideMsg.TotalSlides,
                TimeToDeliver = _speakerSlides.Max(it => it.ReceivedAt) - _speakerSlides.Min(it => it.ReceivedAt)
            });

            if (t is not null)
            {
                await t;
            }
        });

        _speakerSlides.Clear();
    }

    private static byte[] ConcatArrays(IReadOnlyCollection<byte[]> array)
    {
        var payloads = array
            .Select(it => new { Payload = it, it.Length })
            .ToList();
        
        var payloadLength = array.Sum(it => it.Length);
        var result = new byte[payloadLength];

        var lastIndex = 0;

        foreach (var p in payloads)
        {
            p.Payload.CopyTo(result, lastIndex);
            lastIndex += p.Length;
        }
        
        return result;
    }

    public void StartListenSlides(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Func<SlideDto, Task>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback)
    {
        _speakerSlides.Clear();
        _handlersQueue.Clear();
        
        _onListenResultHandler = listenResultCallback;
        _onListenFailedHandler = listenFailedCallback;
        
        _queueWorkerCts = new CancellationTokenSource();
        _queueWorkerTask = Task.Run(HandleMessage, _queueWorkerCts.Token);
        
        _proximityListener.StartListenSpeaker(
            appId: appId,
            speakerIdentifier: speakerIdentifier,
            listenResultCallback: OnScanResult,
            listenFailedCallback: OnScanFailed);
    }

    public void StopListen()
    {
        _proximityListener.StopListen();
        
        _queueWorkerCts.Cancel();
        
        _speakerSlides.Clear();
        _handlersQueue.Clear();
        
        _onListenResultHandler = null;
        _onListenFailedHandler = null;
    }
}