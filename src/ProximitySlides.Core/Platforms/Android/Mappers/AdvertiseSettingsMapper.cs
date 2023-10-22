using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Advertisers;

namespace ProximitySlides.Core.Platforms.Android.Mappers;

public static class AdvertiseSettingsMapper
{
    public static BleAdvertiseSettings? Map(AdvertiseSettings? advertiseSettings)
    {
        if (advertiseSettings is null)
        {
            return null;
        }
        
        // TODO: add mapper
        return new BleAdvertiseSettings();
    }
}