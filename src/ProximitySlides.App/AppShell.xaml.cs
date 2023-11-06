using ProximitySlides.App.Pages;

namespace ProximitySlides.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(SpeakerPage), typeof(SpeakerPage));
        Routing.RegisterRoute(nameof(ListenerPage), typeof(ListenerPage));
        Routing.RegisterRoute(nameof(PresentationPage), typeof(PresentationPage));
    }
}