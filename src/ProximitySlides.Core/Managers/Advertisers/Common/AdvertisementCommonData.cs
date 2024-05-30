namespace ProximitySlides.Core.Managers.Advertisers.Common;

public record AdvertisementCommonData(
    bool IncludeDeviceName,
    bool IncludeTxPowerLevel,
    IEnumerable<ServiceData> ServicesData);