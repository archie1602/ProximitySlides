using ProximitySlides.Core.Managers.Advertisers.Common;

namespace ProximitySlides.Core.Managers.Advertisers.Extended;

public record ExtendedAdvertisementSettings(
    int Interval,
    BleExtendedAdvertiseTx TxPowerLevel,
    AdvertisementBluetoothPhy PrimaryPhy,
    AdvertisementBluetoothPhy SecondaryPhy,
    bool IsConnectable = false);