namespace ProximitySlides.Core.Managers.Advertisers.Classic;

public interface IBleAdvertiser
{
    void StartAdvertising(
        AdvertisementOptions options,
        Action<BleAdvertiseSettings?>? startSuccessCallback = null,
        Action<BleAdvertiseFailure>? startFailureCallback = null);
    
    void StopAdvertising();
}