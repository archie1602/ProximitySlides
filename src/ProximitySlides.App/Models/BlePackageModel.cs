namespace ProximitySlides.App.Models;

public class BlePackageModel
{
    public required string SenderId { get; set; }
    public required int CurrentPage { get; set; }
    public required int TotalPages { get; set; }
    public required byte[] Payload { get; set; }
    public required DateTime ReceivedAt { get; set; }
}

public class BlePackageComparator : Comparer<BlePackageModel>
{
    public override int Compare(BlePackageModel? x, BlePackageModel? y)
    {
        // TODO: fix nullability
        return x.CurrentPage.CompareTo(y.CurrentPage);
    }
}