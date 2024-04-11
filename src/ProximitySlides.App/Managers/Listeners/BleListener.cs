using System.ComponentModel;
using System.Text;

using Android.Bluetooth.LE;
using Android.OS;

using Microsoft.Extensions.Logging;

using ProximitySlides.App.Models;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.App.Managers.Listeners;

public class BleListener(ILogger<BleListener> logger, IBleScanner bleScanner) : IProximityListener
{
    private bool _isSpeakerIdFilterEnabled;
    private string? _listenId;
    private SpeakerIdentifier? _speakerId;

    private const int SpeakerIdLength = 2;
    private const string BaseUuid = "0000{0}-0000-1000-8000-00805F9B34FB";

    private Action<BlePackageMessage>? _onListenResultHandler;
    private Action<ListenFailed>? _onListenFailedHandler;

    private DateTime ConvertFromUnixEpoch(long unixEpochTs)
    {

    }

    private void OnScanResult(BleScanCallbackType callbackType, ScanResult? result)
    {
        var now = DateTime.UtcNow;

        try
        {
            if (result is null)
            {
                return;
            }

            var dataUuid = ParcelUuid.FromString(_listenId);
            var bytes = result.ScanRecord?.GetServiceData(dataUuid);

            if (bytes is null)
            {
                return;
            }

            var speakerId = Encoding.ASCII.GetString(bytes[0..SpeakerIdLength]);

            if (_isSpeakerIdFilterEnabled && speakerId != _speakerId?.SpeakerId)
            {
                return;
            }

            var sendAtBytes = bytes[(SpeakerIdLength + 2)..(SpeakerIdLength + 2 + 8)];
            var sendAtUnixEpoch = BitConverter.ToInt64(sendAtBytes);
            var sendAt = DateTime.UnixEpoch.AddMilliseconds(sendAtUnixEpoch);

            var package = new BlePackageMessage
            {
                SpeakerId = speakerId,
                CurrentPackage = bytes[SpeakerIdLength],
                TotalPackages = bytes[SpeakerIdLength + 1],
                Payload = bytes[(SpeakerIdLength + 2 + 8)..bytes.Length],
                Rssi = result.Rssi,
                TransmissionTime = now - sendAt,
                ReceivedAt = DateTime.UtcNow
            };

            _onListenResultHandler?.Invoke(package);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while listening the message");
        }
    }

    private void OnScanFailed(BleScanFailure errorCode)
    {
        // TODO: think about this mapper layer
        var listenType = errorCode switch
        {
            BleScanFailure.AlreadyStarted => ListenFailed.AlreadyStarted,
            BleScanFailure.ApplicationRegistrationFailed => ListenFailed.ApplicationRegistrationFailed,
            BleScanFailure.InternalError => ListenFailed.InternalError,
            BleScanFailure.FeatureUnsupported => ListenFailed.FeatureUnsupported,
            BleScanFailure.OutOfHardwareResources => ListenFailed.OutOfHardwareResources,
            BleScanFailure.ScanningTooFrequently => ListenFailed.ScanningTooFrequently,
            _ => throw new InvalidEnumArgumentException("Enum out of range")
        };

        _onListenFailedHandler?.Invoke(listenType);
    }

    private static string GetSenderUuid(string appId)
    {
        // TODO: добавить проверку чисел на шестнадцатеричность
        if (appId.Length != 4)
        {
            throw new ArgumentException("AppId must consist of two hexadecimal numbers");
        }

        return string.Format(BaseUuid, appId);
    }

    public void StartListenConcreteSpeaker(
        bool isExtended,
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback)
    {
        _listenId = GetSenderUuid(appId);
        _speakerId = speakerIdentifier;
        _isSpeakerIdFilterEnabled = true;

        var scanConfig = new ScanConfig(
            IsExtended: isExtended,
            Mode: BleScanMode.LowLatency,
            ServiceDataUuid: _listenId);

        _onListenResultHandler = listenResultCallback;
        _onListenFailedHandler = listenFailedCallback;

        bleScanner.StartScan(scanConfig, OnScanResult, OnScanFailed);
    }

    public void StartListenAllSpeakers(
        bool isExtended,
        string appId,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback)
    {
        _listenId = GetSenderUuid(appId);
        _isSpeakerIdFilterEnabled = false;

        var scanConfig = new ScanConfig(
            IsExtended: isExtended,
            Mode: BleScanMode.LowLatency,
            ServiceDataUuid: _listenId);

        _onListenResultHandler = listenResultCallback;
        _onListenFailedHandler = listenFailedCallback;

        bleScanner.StartScan(scanConfig, OnScanResult, OnScanFailed);
    }

    public void StopListen()
    {
        bleScanner.StopScan();

        _isSpeakerIdFilterEnabled = false;
        _listenId = null;
        _speakerId = null;

        _onListenResultHandler = null;
        _onListenFailedHandler = null;
    }
}
