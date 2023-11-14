namespace ProximitySlides.App.Models;

public class SlideStorage
{
    // slide_1.png
    // slide_1.pdf
    public required string FileName { get; set; }
    
    // presentations
    // speakers
    public required string BaseSpeakersDirectory { get; set; }

    // 2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9
    // 430358da-965d-4123-94f3-dc480ac5406b
    public required string BaseCurrentSpeakerDirectory { get; set; }

    // /data/data/com.companyname.proximityslides.app/files/presentations/2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9/slides/slide_1.png
    // /data/data/com.companyname.proximityslides.app/files/speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string AbsoluteStoragePath { get; set; }

    // presentations/2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9/slides/slide_1.png
    // speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string RelativeStoragePath { get; set; }
}