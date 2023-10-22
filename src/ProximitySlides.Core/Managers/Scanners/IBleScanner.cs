using Android.Bluetooth.LE;

namespace ProximitySlides.Core.Managers.Scanners;

public interface IBleScanner
{
    void StartScan(
        ScanConfig scanConfig,
        Action<BleScanCallbackType, ScanResult?>? scanResultCallback = null,
        Action<BleScanFailure>? scanFailedCallback = null);
    
    void StopScan();
}