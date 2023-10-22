using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class ListenerSettings : IBaseSettings
{
    public string SectionName => "Listener";
    
    public TimeSpan MaxInactiveSpeakerTime { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ClearInactiveSpeakersJobDelay { get; set; } = TimeSpan.FromSeconds(2);
}