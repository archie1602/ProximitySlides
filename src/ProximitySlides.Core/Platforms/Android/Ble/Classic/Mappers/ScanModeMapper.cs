using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.Core.Platforms.Android.Ble.Classic.Mappers;

public static class ScanModeMapper
{
    public static ScanMode Map(BleScanMode scanMode)
    {
        return scanMode switch
        {
            BleScanMode.Opportunistic => ScanMode.Opportunistic,
            BleScanMode.LowPower => ScanMode.LowPower,
            BleScanMode.Balanced => ScanMode.Balanced,
            BleScanMode.LowLatency => ScanMode.LowLatency,
            _ => throw new ArgumentOutOfRangeException(nameof(scanMode))
        };
    }
}