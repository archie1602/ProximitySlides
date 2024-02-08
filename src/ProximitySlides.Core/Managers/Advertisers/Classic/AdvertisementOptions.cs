using ProximitySlides.Core.Managers.Advertisers.Common;

namespace ProximitySlides.Core.Managers.Advertisers.Classic;

public record AdvertisementOptions(
    AdvertisementSettings Settings,
    AdvertisementCommonData Data);