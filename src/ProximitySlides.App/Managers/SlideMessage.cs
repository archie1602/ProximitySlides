using System.Text.Json.Serialization;

namespace ProximitySlides.App.Managers;

public class SlideMessage
{
    [JsonPropertyName("u")]
    public string Url { get; set; } = null!;
    
    [JsonPropertyName("c")]
    public byte CurrentSlide { get; set; }
    
    [JsonPropertyName("t")]
    public byte TotalSlides { get; set; }
}