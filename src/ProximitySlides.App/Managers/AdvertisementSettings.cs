using Android.Bluetooth.LE;

namespace ProximitySlides.App.Managers;

public record AdvertisementSettings(
    AdvertiseMode Mode,
    AdvertiseTx TxPowerLevel,
    bool IsConnectable = false);