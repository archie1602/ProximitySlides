using Android.Bluetooth;
using ProximitySlides.Core.Managers.Advertisers.Common;

namespace ProximitySlides.Core.Platforms.Android.Ble.Common.Mappers;

public static class BluetoothPhyMapper
{
    public static BluetoothPhy Map(AdvertisementBluetoothPhy phyType)
    {
        return phyType switch
        {
            AdvertisementBluetoothPhy.Le1m => BluetoothPhy.Le1m, 
            AdvertisementBluetoothPhy.Le2m => BluetoothPhy.Le2m, 
            AdvertisementBluetoothPhy.Le1mMask => BluetoothPhy.Le1mMask,
            AdvertisementBluetoothPhy.Le2mMask => BluetoothPhy.Le2mMask,
            AdvertisementBluetoothPhy.LeCoded => BluetoothPhy.LeCoded,
            AdvertisementBluetoothPhy.LeCodedMask => BluetoothPhy.LeCodedMask,
            _ => throw new ArgumentOutOfRangeException(nameof(phyType))
        };
    }
}