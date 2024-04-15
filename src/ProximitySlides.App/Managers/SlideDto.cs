namespace ProximitySlides.App.Managers;

public record SlideDto(
    string Url,
    byte TotalSlides,
    byte CurrentSlide,
    int PayloadLength,
    TimeSpan TotalTransmissionTime,
    List<int> PackagesRssi);
