namespace ProximitySlides.Core.Managers.Advertisers.Classic;

public record AdvertisementSettings(
    BleAdvertiseMode Mode,
    BleAdvertiseTx TxPowerLevel,
    bool IsConnectable = false);