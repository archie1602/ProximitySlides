using Android.Bluetooth.LE;
using Kotlin;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.Core.Platforms.Android.Mappers;

public static class ScanCallbackTypeMapper
{
    public static BleScanCallbackType Map(ScanCallbackType scanCallbackType)
    {
        return scanCallbackType switch
        {
            ScanCallbackType.AllMatches => BleScanCallbackType.AllMatches,
            ScanCallbackType.FirstMatch => BleScanCallbackType.FirstMatch,
            ScanCallbackType.MatchLost => BleScanCallbackType.MatchLost,
            _ => throw new ArgumentOutOfRangeException(nameof(scanCallbackType))
        };
    }
}