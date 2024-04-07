namespace ProximitySlides.App.Models;

public class SpeakerSlide
{
    public required string FileId { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required SlideStorage Storage { get; set; }
}
