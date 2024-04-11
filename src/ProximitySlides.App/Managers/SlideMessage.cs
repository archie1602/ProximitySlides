namespace ProximitySlides.App.Managers;

public record SlideMessage(
    Uri Url,
    int TotalSlides,
    int CurrentSlide,
    TimeSpan TimeToDeliver,
    int FileIdLength,
    List<int> PackagesRssi);
