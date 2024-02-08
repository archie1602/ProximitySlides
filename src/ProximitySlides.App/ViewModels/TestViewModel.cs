using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Models;
using ProximitySlides.Core;
using ProximitySlides.Core.Managers.Advertisers;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Common;
using SkiaSharp;

namespace ProximitySlides.App.ViewModels;

public partial class TestViewModel : ObservableObject
{
    private readonly IBleAdvertiser _bleAdvertiser;

    public TestViewModel(IBleAdvertiser bleAdvertiser)
    {
        _bleAdvertiser = bleAdvertiser;
    }
    
    [RelayCommand]
    private async Task OnAppearing()
    {
        
    }

    [RelayCommand]
    private async Task OnStartButtonClicked()
    {
        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page)) = 31 - 8 = 23 bytes (payload)
        // 255 - (1 (???) + 16 (uuid))
        
        var payload = Encoding.ASCII.GetBytes(new string('b', 250)); // + 17
        
        var advSettings = new AdvertisementSettings(
            Mode: BleAdvertiseMode.LowLatency,
            TxPowerLevel: BleAdvertiseTx.PowerHigh,
            IsConnectable: false);
        
        var avdOptions = new AdvertisementOptions(
            Settings: advSettings,
            Data: new AdvertisementCommonData(
                IncludeDeviceName: false,
                IncludeTxPowerLevel: false,
                ServicesData: new List<ServiceData> { new(Guid.NewGuid().ToString(), payload) }));
        
        _bleAdvertiser.StartAdvertising(avdOptions);
    }

    [RelayCommand]
    private void OnDisappearing()
    {
    }
}