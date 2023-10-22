using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

public partial class ListenerPage : ContentPage
{
    public ListenerPage(ListenerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}