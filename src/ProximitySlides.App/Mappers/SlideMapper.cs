using ProximitySlides.App.Managers;
using ProximitySlides.App.Models;

namespace ProximitySlides.App.Mappers;

public static class SlideMapper
{
    public static SlideMessage Map(SpeakerSlide slide)
    {
        return new SlideMessage
        {
            Url = slide.Url.ToString(),
            CurrentSlide = (byte)slide.CurrentSlide,
            TotalSlides = (byte)slide.TotalSlides
        };
    }
}