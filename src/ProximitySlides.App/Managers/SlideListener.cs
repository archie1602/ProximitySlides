using System.Text;

using ConcurrentCollections;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Models;

namespace ProximitySlides.App.Managers;

public class SlideListener(
    ILogger<SlideListener> logger,
    IConfiguration configuration,
    IProximityListener proximityListener) : ISlideListener
{
    private readonly ConcurrentHashSet<BlePackageMessage> _speakerPackages = new(comparer: new BlePackageEqualityComparer());

    private Func<SlideMessage, Task>? _onListenResultHandler;
    private Action<ListenFailed>? _onListenFailedHandler;

    private readonly AppSettings _appSettings = configuration.GetConfigurationSettings<AppSettings>();

    private async void OnScanResult(BlePackageMessage package)
    {
        try
        {
            var existsPackage = _speakerPackages.FirstOrDefault(it => it.CurrentPackage == package.CurrentPackage);

            if (existsPackage is null)
            {
                _speakerPackages.Add(package);
            }
            else
            {
                TryUpdateExistingPackage(package, existsPackage);
            }

            if (_speakerPackages.Count > package.TotalPackages)
            {
                _speakerPackages.Clear();
                return;
            }

            if (_speakerPackages.Count < package.TotalPackages)
            {
                return;
            }

            var slideMsg = DecodeSlideMessage();

            if (!Uri.IsWellFormedUriString(slideMsg.Url, UriKind.Absolute))
            {
                _speakerPackages.Clear();
                return;
            }

            var t = InvokeHandler(slideMsg);

            if (t is not null)
            {
                await t;
            }

            _speakerPackages.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while processing the speaker's advertising package: {ErrorMessage}",
                ex.Message);
        }
    }

    private void OnScanFailed(ListenFailed errorCode)
    {
        try
        {
            _onListenFailedHandler?.Invoke(errorCode);
        }
        catch (Exception ex)
        {
            // TODO: add log
        }
    }

    private Task? InvokeHandler(SlideDto slideMsg)
    {
        var ttd = _speakerPackages.Max(it => it.ReceivedAt) - _speakerPackages.Min(it => it.ReceivedAt);

        return _onListenResultHandler?.Invoke(
            new SlideMessage(
                Url: new Uri(slideMsg.Url),
                CurrentSlide: slideMsg.CurrentSlide,
                TotalSlides: slideMsg.TotalSlides,
                TimeToDeliver: ttd));
    }

    private SlideDto DecodeSlideMessage()
    {
        var payloads = _speakerPackages
            .Select(it => it.Payload)
            .ToList();

        var payloadBytes = ConcatArrays(payloads);
        var fileId = Encoding.ASCII.GetString(payloadBytes[2..]);

        var slideMsg = new SlideDto(
            Url: $"{_appSettings.FileSharingUrlPrefix}{fileId}",
            TotalSlides: payloadBytes[0],
            CurrentSlide: payloadBytes[1]);

        return slideMsg;
    }

    private void TryUpdateExistingPackage(BlePackageMessage newPackage, BlePackageMessage oldPackage)
    {
        if (oldPackage.TotalPackages == newPackage.TotalPackages
            && oldPackage.Payload.SequenceEqual(newPackage.Payload))
        {
            oldPackage.ReceivedAt = newPackage.ReceivedAt;
            return;
        }

        _speakerPackages.Clear();
        _speakerPackages.Add(newPackage);
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
        Func<SlideMessage, Task>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback)
    {
        _speakerPackages.Clear();

        _onListenResultHandler = listenResultCallback;
        _onListenFailedHandler = listenFailedCallback;

        proximityListener.StartListenConcreteSpeaker(
            isExtended: isExtended,
            appId: appId,
            speakerIdentifier: speakerIdentifier,
            listenResultCallback: OnScanResult,
            listenFailedCallback: OnScanFailed);
    }

    public void StopListen()
    {
        proximityListener.StopListen();

        _speakerPackages.Clear();

        _onListenResultHandler = null;
        _onListenFailedHandler = null;
    }
}
