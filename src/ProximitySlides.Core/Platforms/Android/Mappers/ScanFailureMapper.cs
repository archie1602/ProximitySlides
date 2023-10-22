using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Scanners;

namespace ProximitySlides.Core.Platforms.Android.Mappers;

public static class ScanFailureMapper
{
    public static BleScanFailure Map(ScanFailure scanFailure)
    {
        return scanFailure switch
        {
            ScanFailure.AlreadyStarted => BleScanFailure.AlreadyStarted,
            ScanFailure.ApplicationRegistrationFailed => BleScanFailure.ApplicationRegistrationFailed,
            ScanFailure.InternalError => BleScanFailure.InternalError,
            ScanFailure.FeatureUnsupported => BleScanFailure.FeatureUnsupported,
            ScanFailure.OutOfHardwareResources => BleScanFailure.OutOfHardwareResources,
            ScanFailure.ScanningTooFrequently => BleScanFailure.ScanningTooFrequently,
            _ => throw new ArgumentOutOfRangeException(nameof(scanFailure))
        };
    }
}