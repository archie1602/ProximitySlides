namespace ProximitySlides.App.Metrics;

public class BleMetric
{
    // [Index(0)]
    public string DeviceName { get; set; } = null!;

    public int PayloadLength { get; set; }

    public double TransferTime { get; set; }

    public bool IsExtendedAdvertising { get; set; }

    public string BleAdvertiseMode { get; set; } = null!;

    public string BleAdvertiseTx { get; set; } = null!;

    public string BleScanMode { get; set; } = null!;

    public int DelayBetweenCirclesMs { get; set; }

    public int DelayBetweenPackagesMs { get; set; }

    public int MinRssi { get; set; }

    public int MaxRssi { get; set; }

    public double AverageRssi { get; set; }

    public DateTime CreatedAt { get; set; }

    public long Ticks { get; set; }
}
