namespace ProximitySlides.App.Models;

public class Slide
{
    public required Uri Url { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required SlideStorage Storage { get; set; }
    public required TimeSpan TimeToDeliver { get; set; }
}