using System.Text.Json.Serialization;

namespace ProximitySlides.App.Models;

public class PresentationModel
{
    [JsonPropertyName("u")]
    public string Url { get; set; } = null!;
}