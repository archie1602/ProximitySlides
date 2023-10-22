namespace ProximitySlides.App.Managers;

public record AdvertisementData(
    bool IncludeDeviceName,
    bool IncludeTxPowerLevel,
    IEnumerable<ServiceData> ServicesData);