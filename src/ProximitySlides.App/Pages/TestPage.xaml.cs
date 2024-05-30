using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class TestPage : ContentPage
{
    public TestPage(TestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}