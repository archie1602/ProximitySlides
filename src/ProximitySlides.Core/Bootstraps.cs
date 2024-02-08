using ProximitySlides.Core.Managers;
using ProximitySlides.Core.Managers.Advertisers;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Extended;
using ProximitySlides.Core.Managers.Scanners;
using ProximitySlides.Core.Platforms.Android;
using ProximitySlides.Core.Platforms.Android.Ble.Classic;
using ProximitySlides.Core.Platforms.Android.Ble.Common;
using ProximitySlides.Core.Platforms.Android.Ble.Extended;

namespace ProximitySlides.Core;

public static class Bootstraps
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IBleScanner, BleScanner>();
        services.AddSingleton<IBleAdvertiser, BleAdvertiser>();
        services.AddSingleton<IBleExtendedAdvertiser, BleExtendedAdvertiser>();
        
        return services;
    }
}