namespace ProximitySlides.App.Managers;

public record SlideDto(
    string Url,
    byte TotalSlides,
    byte CurrentSlide,
    int FileIdLength,
    List<int> PackagesRssi);
