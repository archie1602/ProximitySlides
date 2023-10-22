using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class ListenerDetailsPage : ContentPage
{
    public ListenerDetailsPage(ListenerDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}