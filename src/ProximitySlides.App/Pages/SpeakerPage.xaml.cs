using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Pages;

// ReSharper disable once RedundantExtendsListEntry
public partial class SpeakerPage : ContentPage
{
    public SpeakerPage(SpeakerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}