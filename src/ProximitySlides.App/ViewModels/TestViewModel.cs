using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Models;
using ProximitySlides.Core;
using ProximitySlides.Core.Managers.Advertisers;
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
        var payload = Encoding.ASCII.GetBytes(new string('a', 1));
        
        var advSettings = new AdvertisementSettings(
            Mode: BleAdvertiseMode.LowLatency,
            TxPowerLevel: BleAdvertiseTx.PowerHigh,
            IsConnectable: false);
        
        var avdOptions = new AdvertisementOptions(
            Settings: advSettings,
            Data: new AdvertisementData(
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