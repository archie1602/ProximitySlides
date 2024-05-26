using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ProximitySlides.Core.Managers.Advertisers.Classic;

namespace ProximitySlides.App.ViewModels;

public partial class TestViewModel : ObservableObject
{
    private readonly IBleAdvertiser _bleAdvertiser;

    public TestViewModel(IBleAdvertiser bleAdvertiser)
    {
        _bleAdvertiser = bleAdvertiser;
    }

    [ObservableProperty] private string _syncTime = null!;

    [ObservableProperty] private int _bleVersion;

    [ObservableProperty] private int _scanMode;

    [ObservableProperty] private int _broadcastDelayBetweenCirclesMs;

    [ObservableProperty] private int _broadcastDelayBetweenPackagesMs;

    [ObservableProperty] private int _advertisingMode;

    [ObservableProperty] private int _advertisingTx;

    [RelayCommand]
    private async Task OnAppearing()
    {
    }

    [RelayCommand]
    private async Task OnSaveSettingsButtonClicked()
    {
        // if (BroadcastDelayBetweenPackagesMs >= 0)
        // {
        //     AppParameters.BroadcastDelayBetweenPackagesMs = BroadcastDelayBetweenPackagesMs;
        // }
        //
        // if (BroadcastPeriodBetweenCirclesMs >= 0)
        // {
        //     AppParameters.BroadcastDelayBetweenCirclesMs = BroadcastPeriodBetweenCirclesMs;
        // }
        //
        // if (IsExtendedAdvertising == 0)
        // {
        //     AppParameters.IsExtendedAdvertising = false;
        //     IsExtendedAdvertising = 0;
        // }
        // else
        // {
        //     AppParameters.IsExtendedAdvertising = true;
        //     IsExtendedAdvertising = 1;
        // }
        //
        // BroadcastDelayBetweenPackagesMs = AppParameters.BroadcastDelayBetweenPackagesMs;
        // BroadcastPeriodBetweenCirclesMs = AppParameters.BroadcastDelayBetweenCirclesMs;

        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page)) = 31 - 8 = 23 bytes (payload)
        // 255 - (1 (???) + 16 (uuid))

        //var payload = Encoding.ASCII.GetBytes(new string('b', 250)); // + 17

        //var advSettings = new AdvertisementSettings(
        //    Mode: BleAdvertiseMode.LowLatency,
        //    TxPowerLevel: BleAdvertiseTx.PowerHigh,
        //    IsConnectable: false);

        //var avdOptions = new AdvertisementOptions(
        //    Settings: advSettings,
        //    Data: new AdvertisementCommonData(
        //        IncludeDeviceName: false,
        //        IncludeTxPowerLevel: false,
        //        ServicesData: new List<ServiceData> { new(Guid.NewGuid().ToString(), payload) }));

        //_bleAdvertiser.StartAdvertising(avdOptions);
    }

    [RelayCommand]
    private async Task OnStartScanningButtonClicked()
    {
    }

    [RelayCommand]
    private async Task OnShareMetricsFileButtonClicked()
    {
    }

    [RelayCommand]
    private void OnDisappearing()
    {
    }
}
