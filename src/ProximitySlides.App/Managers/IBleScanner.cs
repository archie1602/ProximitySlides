using Android.Bluetooth.LE;

namespace ProximitySlides.App.Managers;

public interface IBleScanner
{
    void StartScan(
        ScanConfig scanConfig,
        Action<ScanCallbackType, ScanResult?>? scanResultCallback = null,
        Action<ScanFailure>? scanFailedCallback = null);
    
    void StopScan();
}