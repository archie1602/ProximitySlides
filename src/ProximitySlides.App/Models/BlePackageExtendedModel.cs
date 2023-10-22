namespace ProximitySlides.App.Models;

public class BlePackageExtendedModel
{
    public required string SenderId { get; set; }
    public required int CurrentPage { get; set; }
    public required int TotalPages { get; set; }
    public required byte[] Payload { get; set; }
    public required DateTime ReceivedAt { get; set; }
}