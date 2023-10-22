namespace ProximitySlides.App.Managers;

public class ProximityMessage
{
    public string SenderId { get; set; } = null!;
    public byte[]? Data { get; set; }
}