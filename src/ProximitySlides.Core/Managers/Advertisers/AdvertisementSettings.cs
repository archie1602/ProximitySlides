namespace ProximitySlides.Core.Managers.Advertisers;

public record AdvertisementSettings(
    BleAdvertiseMode Mode,
    BleAdvertiseTx TxPowerLevel,
    bool IsConnectable = false);