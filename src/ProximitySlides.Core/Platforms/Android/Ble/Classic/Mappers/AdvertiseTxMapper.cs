using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Advertisers.Classic;

namespace ProximitySlides.Core.Platforms.Android.Ble.Classic.Mappers;

public static class AdvertiseTxMapper
{
    public static AdvertiseTx Map(BleAdvertiseTx advertiseTx)
    {
        return advertiseTx switch
        {
            BleAdvertiseTx.PowerUltraLow => AdvertiseTx.PowerUltraLow,
            BleAdvertiseTx.PowerLow => AdvertiseTx.PowerLow,
            BleAdvertiseTx.PowerMedium => AdvertiseTx.PowerMedium,
            BleAdvertiseTx.PowerHigh => AdvertiseTx.PowerHigh,
            _ => throw new ArgumentOutOfRangeException(nameof(advertiseTx))
        };
    }
}