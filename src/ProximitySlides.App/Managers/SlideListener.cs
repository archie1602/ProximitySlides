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
            var guid = Guid.NewGuid();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("(Guid: {Id}): START. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
            
            if (!_handlersQueue.TryDequeue(out var package))
            {
                // TODO: move to config
                // await Task.Delay(TimeSpan.FromMilliseconds(100));
                
                _logger.LogInformation("(Guid: {Id}): COULDN'T dequeue element from queue", guid);
                
                _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                watch.Stop();
                
                continue;
            }
            
            try
            {
                var tmpPackage = _speakerSlides
                    .FirstOrDefault(it => it.CurrentPage == package.CurrentPage);

                if (tmpPackage is not null)
                {
                    _logger.LogInformation("(Guid: {Id}): TryUpdateExistingPackage", guid);
                    _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                    watch.Stop();
                    
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
                    _logger.LogInformation("(Guid: {Id}): !isAllTotalPagesEqual", guid);
                    _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                    watch.Stop();
                    
                    // TODO: skip all and start again...
                    _speakerSlides.Clear();
                    _speakerSlides.Add(package);
                    
                    continue;
                }

                if (_speakerSlides.Count != package.TotalPages)
                {
                    _logger.LogInformation("(Guid: {Id}): _speakerSlides.Count != package.TotalPages", guid);
                    _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                    watch.Stop();
                    
                    continue;
                }
            
                var slideMsg = TryDeserializeSlideMessage();

                if (slideMsg is null || !Uri.IsWellFormedUriString(slideMsg.Url, UriKind.Absolute))
                {
                    _logger.LogInformation("(Guid: {Id}): slideMsg is null || !Uri.IsWellFormedUriString(slideMsg.Url, UriKind.Absolute)", guid);
                    _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                    watch.Stop();
                    
                    // TODO: add log
                    continue;
                }
            
                await InvokeHandler(slideMsg);
                
                _logger.LogInformation("(Guid: {Id}): HAPPY PATH InvokeHandler", guid);
                _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                watch.Stop();
            }
            catch (Exception e)
            {
                _logger.LogInformation("(Guid: {Id}): Exception", guid);
                _logger.LogInformation("(Guid: {Id}): END. TOTAL ELEMENTS: {Total}", guid, _handlersQueue.Count);
                watch.Stop();
                
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
        bool isExtended,
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
            isExtended: isExtended,
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