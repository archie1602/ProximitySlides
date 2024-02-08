using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Advertisers.Extended;

namespace ProximitySlides.Core.Platforms.Android.Ble.Extended.Mappers;

public static class ExtendedAdvertiseTxMapper
{
    public static AdvertiseTxPower Map(BleExtendedAdvertiseTx advertiseTx)
    {
        return advertiseTx switch
        {
            BleExtendedAdvertiseTx.Max => AdvertiseTxPower.Max,
            BleExtendedAdvertiseTx.High => AdvertiseTxPower.High,
            BleExtendedAdvertiseTx.UltraLow => AdvertiseTxPower.UltraLow,
            BleExtendedAdvertiseTx.Low => AdvertiseTxPower.Low,
            BleExtendedAdvertiseTx.Medium => AdvertiseTxPower.Medium,
            BleExtendedAdvertiseTx.Min => AdvertiseTxPower.Min,
            _ => throw new ArgumentOutOfRangeException(nameof(advertiseTx))
        };
    }
}