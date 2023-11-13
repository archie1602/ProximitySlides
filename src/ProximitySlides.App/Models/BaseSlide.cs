namespace ProximitySlides.App.Models;

public class BaseSlide
{
    public required Uri Url { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required SlideStorage Storage { get; set; }
}