namespace ProximitySlides.App.Managers;

public class SlideDto
{
    public required Uri Url { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required TimeSpan TimeToDeliver { get; set; }
}