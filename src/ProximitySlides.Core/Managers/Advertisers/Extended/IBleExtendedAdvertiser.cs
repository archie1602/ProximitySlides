using ProximitySlides.Core.Managers.Advertisers.Classic;

namespace ProximitySlides.Core.Managers.Advertisers.Extended;

public interface IBleExtendedAdvertiser
{
    void StartAdvertising(
        AdvertisementExtendedOptions options,
        Action<BleAdvertiseSettings?>? startSuccessCallback = null,
        Action<BleAdvertiseFailure>? startFailureCallback = null);
    
    void StopAdvertising();

    int GetMaxAdvertisingDataLength();
}