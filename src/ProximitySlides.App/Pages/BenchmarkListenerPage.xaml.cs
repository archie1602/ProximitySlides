using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class BenchmarkListenerPage : ContentPage
{
    public BenchmarkListenerPage(BenchmarkListenerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

