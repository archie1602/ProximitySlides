using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class PresentationSettings : IBaseSettings
{
    public string SectionName => "Presentation";

    public TimeSpan MaxInactiveSpeakerTime { get; set; }

    public TimeSpan CheckSpeakerActivityJobDelay { get; set; }
}
