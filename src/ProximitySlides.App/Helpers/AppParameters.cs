using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Extended;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.App.Helpers;

public static class AppParameters
{
    public static int BroadcastDelayBetweenCirclesMs { get; set; } = 200;
    public static int BroadcastDelayBetweenPackagesMs { get; set; } = 100;
    public static bool IsExtendedAdvertising { get; set; } = false;
    public static BleAdvertiseMode BleAdvertiseMode { get; set; } = BleAdvertiseMode.Balanced;
    public static int ExtendedBleAdvertiseMode { get; set; } = ExtendedAdvertisementInterval.IntervalMedium;
    public static BleAdvertiseTx BleAdvertiseTx { get; set; } = BleAdvertiseTx.PowerLow;
    public static BleExtendedAdvertiseTx ExtendedBleAdvertiseTx { get; set; } = BleExtendedAdvertiseTx.Low;
    public static BleScanMode BleScanMode { get; set; } = BleScanMode.Balanced;
}
