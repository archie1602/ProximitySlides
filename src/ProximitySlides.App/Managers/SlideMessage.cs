namespace ProximitySlides.App.Managers;

public class SlideMessage
{
    public string Url { get; set; } = null!;

    public byte CurrentSlide { get; set; }

    public byte TotalSlides { get; set; }
}
