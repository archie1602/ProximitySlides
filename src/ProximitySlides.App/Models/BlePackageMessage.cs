namespace ProximitySlides.App.Models;

public class BlePackageMessage
{
    public required string SpeakerId { get; init; }
    public required int CurrentPackage { get; set; }
    public required int TotalPackages { get; set; }
    public required byte[] Payload { get; set; }
    public required int Rssi { get; set; }
    public required DateTime ReceivedAt { get; set; }
}

public class BlePackageComparator : Comparer<BlePackageMessage>
{
    public override int Compare(BlePackageMessage? x, BlePackageMessage? y)
    {
        // TODO: fix nullability
        return x.CurrentPackage.CompareTo(y.CurrentPackage);
    }
}

public sealed class BlePackageEqualityComparer : IEqualityComparer<BlePackageMessage>
{
    public bool Equals(BlePackageMessage? x, BlePackageMessage? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.CurrentPackage == y.CurrentPackage;
    }

    public int GetHashCode(BlePackageMessage obj)
    {
        return obj.CurrentPackage.GetHashCode();
    }
}
