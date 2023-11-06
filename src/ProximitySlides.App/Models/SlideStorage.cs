namespace ProximitySlides.App.Models;

public class SlideStorage
{
    // slide_1.pdf
    public required string FileName { get; set; }
    
    // speakers
    public required string BaseSpeakersDirectory { get; set; }

    // 430358da-965d-4123-94f3-dc480ac5406b
    public required string BaseCurrentSpeakerDirectory { get; set; }

    // /data/data/com.companyname.proximityslides.app/files/speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string AbsoluteStoragePath { get; set; }

    // speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string RelativeStoragePath { get; set; }
}