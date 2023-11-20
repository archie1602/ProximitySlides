using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class BrowserPage : ContentPage
{
    public BrowserPage(BrowserViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}