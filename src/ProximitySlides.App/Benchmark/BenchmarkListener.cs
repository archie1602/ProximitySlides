using System.ComponentModel;

using Android.Bluetooth.LE;
using Android.OS;

using ConcurrentCollections;

using Microsoft.Extensions.Logging;

using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Metrics;
using ProximitySlides.App.ViewModels;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.App.Benchmark;

public class BenchmarkListener : IBenchmarkListener
{
    private const string BaseUuid = "0000{0}-0000-1000-8000-00805F9B34FB";
    private string? _listenId;

    private readonly ConcurrentHashSet<BenchmarkListenerBleMessage> _speakerPackages = new(comparer: new BenchmarkBlePackageEqualityComparer());

    private Action<BenchmarkListenerMessage>? _onListenResultHandler;
    private Action<ListenFailed>? _onListenFailedHandler;

    private readonly IBleScanner _bleScanner;
    private readonly ILogger<BenchmarkListener> _logger;

    public BenchmarkListener(IBleScanner bleScanner)
    {
        _bleScanner = bleScanner;
    }

    private void OnScanResult(BleScanCallbackType callbackType, ScanResult? result)
    {
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

            var sendAtBytes = bytes[2..(2 + 8)];
            var sendAtUnixEpoch = BitConverter.ToInt64(sendAtBytes);
            var sentAt = DateTime.UnixEpoch.AddMilliseconds(sendAtUnixEpoch);

            var package = new BenchmarkListenerBleMessage
            {
                CurrentPackage = bytes[0],
                TotalPackages = bytes[1],
                Payload = bytes[(2 + 8)..bytes.Length],
                Rssi = result.Rssi,
                SentAt = sentAt,
                ReceivedAt = DateTime.UtcNow
            };

            /////////////////////////////////////////////////////////

            var existsPackage = _speakerPackages
                .FirstOrDefault(it => it.CurrentPackage == package.CurrentPackage);

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

            var payloads = _speakerPackages
                .OrderBy(it => it.CurrentPackage)
                .Select(it => it.Payload)
                .ToList();

            var payloadBytes = ConcatArrays(payloads);

            var slideMsg = new BenchmarkListenerMessage(
                Message: payloadBytes,
                TotalTransmissionTime: DateTime.UtcNow - _speakerPackages.Select(it => it.SentAt).Min(),
                PackagesRssi: _speakerPackages
                    .OrderBy(it => it.CurrentPackage)
                    .Select(it => it.Rssi)
                    .ToList());

            if (!BenchmarkSpeakerViewModel.TestCases.TryGetValue(payloadBytes.Length, out var testCase)
                || !CheckIfArraysAreEqual(payloadBytes, testCase))
            {
                _speakerPackages.Clear();
                return;
            }

            var now = DateTime.Now;

            BleMetricsHandler.SaveMetrics(
                new BleMetric
                {
                    DeviceName = DeviceInfo.Current.Name,
                    PayloadLength = slideMsg.Message.Length,
                    TransferTime = slideMsg.TotalTransmissionTime.TotalMilliseconds,
                    IsExtendedAdvertising = AppParameters.IsExtendedAdvertising,
                    BleAdvertiseMode = AppParameters.BleAdvertiseMode.ToString(),
                    BleAdvertiseTx = AppParameters.BleAdvertiseTx.ToString(),
                    BleScanMode = AppParameters.BleScanMode.ToString(),
                    DelayBetweenCirclesMs = AppParameters.BroadcastDelayBetweenCirclesMs,
                    DelayBetweenPackagesMs = AppParameters.BroadcastDelayBetweenPackagesMs,
                    MinRssi = slideMsg.PackagesRssi.Min(),
                    MaxRssi = slideMsg.PackagesRssi.Max(),
                    AverageRssi = slideMsg.PackagesRssi.Average(),
                    CreatedAt = now,
                    Ticks = now.Ticks
                });

            _onListenResultHandler?.Invoke(slideMsg);
            _speakerPackages.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while listening the message");
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

    private static bool CheckIfArraysAreEqual(byte[] arr1, byte[] arr2)
    {
        if (arr1.Length != arr2.Length)
        {
            return false;
        }

        for (var i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i])
            {
                return false;
            }
        }

        return true;
    }

    private void TryUpdateExistingPackage(BenchmarkListenerBleMessage newPackage, BenchmarkListenerBleMessage oldPackage)
    {
        if (oldPackage.TotalPackages == newPackage.TotalPackages
            && CheckIfArraysAreEqual(oldPackage.Payload, newPackage.Payload))
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

    private static string GetSenderUuid(string appId)
    {
        if (appId.Length != 4)
        {
            throw new ArgumentException("AppId must consist of two hexadecimal numbers");
        }

        return string.Format(BaseUuid, appId);
    }

    public void StartListen(
        bool isExtended,
        string appId,
        Action<BenchmarkListenerMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback)
    {
        _listenId = GetSenderUuid(appId);

        var scanConfig = new ScanConfig(
            IsExtended: isExtended,
            Mode: AppParameters.BleScanMode,
            ServiceDataUuid: _listenId);

        _onListenResultHandler = listenResultCallback;
        _onListenFailedHandler = listenFailedCallback;

        _bleScanner.StartScan(scanConfig, OnScanResult, OnScanFailed);
    }

    public void StopListen()
    {
        _bleScanner.StopScan();

        _listenId = null;

        _onListenResultHandler = null;
        _onListenFailedHandler = null;

        _speakerPackages.Clear();

        _onListenResultHandler = null;
        _onListenFailedHandler = null;
    }
}
