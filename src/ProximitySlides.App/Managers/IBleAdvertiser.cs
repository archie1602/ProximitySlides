using Android.Bluetooth.LE;

namespace ProximitySlides.App.Managers;

public interface IBleAdvertiser
{
    void StartAdvertising(
        AdvertisementOptions options,
        Action<AdvertiseSettings?>? startSuccessCallback = null,
        Action<AdvertiseFailure>? startFailureCallback = null);
    
    void StopAdvertising();
}