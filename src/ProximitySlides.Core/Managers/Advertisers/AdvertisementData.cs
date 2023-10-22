namespace ProximitySlides.Core.Managers.Advertisers;

public record AdvertisementData(
    bool IncludeDeviceName,
    bool IncludeTxPowerLevel,
    IEnumerable<ServiceData> ServicesData);