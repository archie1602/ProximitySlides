using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class PresentationPage : ContentPage
{
    public PresentationPage(PresentationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}