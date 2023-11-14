using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public partial class TestViewModel : ObservableObject
{
    public TestViewModel()
    {
        
    }
    
    [ObservableProperty] private ImageSource _activeSlide;

    [RelayCommand]
    private async Task OnAppearing()
    {
        var pathToSlide = Path.Combine(FileSystem.Current.AppDataDirectory, "presentations", "2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9", "slides", "slide_2.png");
        ActiveSlide = ImageSource.FromFile(pathToSlide);
    }

    [RelayCommand]
    private void OnDisappearing()
    {
        
    }
}