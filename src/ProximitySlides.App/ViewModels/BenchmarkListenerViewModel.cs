using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Configuration;

using ProximitySlides.App.Applications;
using ProximitySlides.App.Benchmark;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Extended;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.App.ViewModels;

public partial class BenchmarkListenerViewModel : ObservableObject
{
    private readonly IBenchmarkListener _benchmarkListener;
    private readonly AppSettings _appSettings;

    private static readonly string MetricBasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ble_metrics.csv");

    private bool _isScanning;

    [ObservableProperty] private string _syncTime = null!;

    [ObservableProperty] private int _bleVersion;

    [ObservableProperty] private int _scanMode;

    [ObservableProperty] private int _broadcastDelayBetweenCirclesMs;

    [ObservableProperty] private int _broadcastDelayBetweenPackagesMs;

    [ObservableProperty] private int _advertisingMode;

    [ObservableProperty] private int _advertisingTx;

    public BenchmarkListenerViewModel(
        IBenchmarkListener benchmarkListener,
        IConfiguration configuration)
    {
        SyncTime = "Init";

        BleVersion = AppParameters.IsExtendedAdvertising ? 5 : 4;
        ScanMode = (int)AppParameters.BleScanMode;

        BroadcastDelayBetweenCirclesMs = AppParameters.BroadcastDelayBetweenCirclesMs;
        BroadcastDelayBetweenPackagesMs = AppParameters.BroadcastDelayBetweenPackagesMs;
        AdvertisingMode = AppParameters.IsExtendedAdvertising ? AppParameters.ExtendedBleAdvertiseMode : (int)AppParameters.BleAdvertiseMode;
        AdvertisingTx = AppParameters.IsExtendedAdvertising ? (int)AppParameters.ExtendedBleAdvertiseTx : (int)AppParameters.BleAdvertiseTx;

        _benchmarkListener = benchmarkListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
    }

    private void OnReceivedSlide(BenchmarkListenerMessage benchmarkMsg)
    {
        try
        {
            var ttt = benchmarkMsg.TotalTransmissionTime;

            SyncTime = $"{ttt.Seconds}:{ttt.Milliseconds}:{ttt.Microseconds}|{benchmarkMsg.PackagesRssi.Min()};{benchmarkMsg.PackagesRssi.Max()}|{benchmarkMsg.Message.Length}";
        }
        catch (Exception ex)
        {
        }
    }

    private void OnListenFailed(ListenFailed errorCode)
    {
        // TODO:
    }

    [RelayCommand]
    private async Task OnStartScanningButtonClicked()
    {
        if (_isScanning)
        {
            _benchmarkListener.StopListen();
            _isScanning = false;
            SyncTime = "Stop scanning...";

            return;
        }

        _benchmarkListener.StartListen(
            isExtended: AppParameters.IsExtendedAdvertising,
            appId: _appSettings.AppAdvertiserId,
            listenResultCallback: OnReceivedSlide,
            listenFailedCallback: OnListenFailed);

        _isScanning = true;
        SyncTime = "Start scanning...";
    }

    [RelayCommand]
    private async Task OnShareMetricsFileButtonClicked()
    {
        if (!File.Exists(MetricBasePath))
        {
            await Shell.Current.DisplayAlert("Not Found", "File not found", "ok");
            return;
        }

        await Share.Default.RequestAsync(
            new ShareFileRequest { Title = "ble_metrics.csv", File = new ShareFile(MetricBasePath) });
    }

    [RelayCommand]
    private async Task OnRemoveMetricsFileButtonClicked()
    {
        if (!File.Exists(MetricBasePath))
        {
            await Shell.Current.DisplayAlert("Not Found", "File not found", "ok");
            return;
        }

        File.Delete(MetricBasePath);
    }

    [RelayCommand]
    private async Task OnSaveSettingsButtonClicked()
    {
        AppParameters.IsExtendedAdvertising = BleVersion switch
        {
            4 => false,
            5 => true,
            _ => false
        };

        AppParameters.BleScanMode = ScanMode switch
        {
            0 => BleScanMode.Opportunistic,
            1 => BleScanMode.LowPower,
            2 => BleScanMode.Balanced,
            3 => BleScanMode.LowLatency,
            _ => BleScanMode.Opportunistic
        };

        if (BroadcastDelayBetweenCirclesMs >= 0)
        {
            AppParameters.BroadcastDelayBetweenCirclesMs = BroadcastDelayBetweenCirclesMs;
        }

        if (BroadcastDelayBetweenPackagesMs >= 0)
        {
            AppParameters.BroadcastDelayBetweenPackagesMs = BroadcastDelayBetweenPackagesMs;
        }

        if (AppParameters.IsExtendedAdvertising)
        {
            AppParameters.ExtendedBleAdvertiseMode = AdvertisingMode switch
            {
                1600 => ExtendedAdvertisementInterval.IntervalHigh,
                160 => ExtendedAdvertisementInterval.IntervalLow,
                16777215 => ExtendedAdvertisementInterval.IntervalMax,
                400 => ExtendedAdvertisementInterval.IntervalMedium,
                161 => ExtendedAdvertisementInterval.IntervalMin,
                _ => ExtendedAdvertisementInterval.IntervalHigh
            };

            AdvertisingMode = AppParameters.ExtendedBleAdvertiseMode;

            AppParameters.ExtendedBleAdvertiseTx = AdvertisingTx switch
            {
                0 => BleExtendedAdvertiseTx.Min,
                1 => BleExtendedAdvertiseTx.UltraLow,
                2 => BleExtendedAdvertiseTx.Low,
                3 => BleExtendedAdvertiseTx.Medium,
                4 => BleExtendedAdvertiseTx.High,
                5 => BleExtendedAdvertiseTx.Max,
                _ => BleExtendedAdvertiseTx.Min
            };

            AdvertisingTx = (int)AppParameters.ExtendedBleAdvertiseTx;
        }
        else
        {
            AppParameters.BleAdvertiseMode = AdvertisingMode switch
            {
                0 => BleAdvertiseMode.LowPower,
                1 => BleAdvertiseMode.Balanced,
                2 => BleAdvertiseMode.LowLatency,
                _ => BleAdvertiseMode.LowPower
            };

            AdvertisingMode = (int)AppParameters.BleAdvertiseMode;

            AppParameters.BleAdvertiseTx = AdvertisingTx switch
            {
                0 => BleAdvertiseTx.PowerUltraLow,
                1 => BleAdvertiseTx.PowerLow,
                2 => BleAdvertiseTx.PowerMedium,
                3 => BleAdvertiseTx.PowerHigh,
                _ => BleAdvertiseTx.PowerUltraLow
            };

            AdvertisingTx = (int)AppParameters.BleAdvertiseTx;
        }

        BleVersion = AppParameters.IsExtendedAdvertising ? 5 : 4;
        ScanMode = (int)AppParameters.BleScanMode;
        BroadcastDelayBetweenCirclesMs = AppParameters.BroadcastDelayBetweenCirclesMs;
        BroadcastDelayBetweenPackagesMs = AppParameters.BroadcastDelayBetweenPackagesMs;
    }
}
