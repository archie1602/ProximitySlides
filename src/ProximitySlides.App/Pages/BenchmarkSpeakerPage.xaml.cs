using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class BenchmarkSpeakerPage : ContentPage
{
    public BenchmarkSpeakerPage(BenchmarkSpeakerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

