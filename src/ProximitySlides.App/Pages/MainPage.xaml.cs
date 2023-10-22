using MainViewModel = ProximitySlides.App.ViewModels.MainViewModel;

namespace ProximitySlides.App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}