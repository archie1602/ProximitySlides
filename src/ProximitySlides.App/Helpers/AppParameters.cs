using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.App.Helpers;

public static class AppParameters
{
    public static int BroadcastDelayBetweenCirclesMs { get; set; } = 100;
    public static int BroadcastDelayBetweenPackagesMs { get; set; } = 100;
    public static bool IsExtendedAdvertising { get; set; } = false;
    public static BleAdvertiseMode BleAdvertiseMode { get; set; } = BleAdvertiseMode.LowLatency;
    public static BleAdvertiseTx BleAdvertiseTx { get; set; } = BleAdvertiseTx.PowerHigh;
    public static BleScanMode BleScanMode { get; set; } = BleScanMode.LowLatency;
}
