namespace ProximitySlides.App.Models;

public record ListenerSlide(
    Uri Url,
    int TotalSlides,
    int CurrentSlide,
    SlideStorage Storage,
    TimeSpan TotalTransmissionTime);
