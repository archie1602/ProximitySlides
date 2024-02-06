using ProximitySlides.Core.Managers;
using ProximitySlides.Core.Managers.Advertisers;
using ProximitySlides.Core.Managers.Scanners;
using ProximitySlides.Core.Platforms.Android;

namespace ProximitySlides.Core;

public static class Bootstraps
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IBleScanner, BleScanner>();
        services.AddSingleton<IBleAdvertiser, BleExtendedAdvertiser>();
        
        return services;
    }
}