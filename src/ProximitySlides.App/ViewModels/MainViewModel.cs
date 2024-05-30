using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProximitySlides.App.Pages;

namespace ProximitySlides.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToSpeaker()
    {
        await Shell.Current.GoToAsync(nameof(BrowserPage));
    }

    [RelayCommand]
    private async Task NavigateToListener()
    {
        await Shell.Current.GoToAsync(nameof(ListenerPage));
    }

    [RelayCommand]
    private async Task NavigateToBenchmarkSpeaker()
    {
        await Shell.Current.GoToAsync(nameof(BenchmarkSpeakerPage));
    }

    [RelayCommand]
    private async Task NavigateToBenchmarkListener()
    {
        await Shell.Current.GoToAsync(nameof(BenchmarkListenerPage));
    }
}
