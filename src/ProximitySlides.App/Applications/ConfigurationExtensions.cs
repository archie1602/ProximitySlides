using Microsoft.Extensions.Configuration;

namespace ProximitySlides.App.Applications;

public static class ConfigurationExtensions
{
    public static T GetConfigurationSettings<T>(this IConfiguration configuration)
        where T : IBaseSettings
    {
        var settingsInstance = Activator.CreateInstance(typeof(T)) as IBaseSettings ??
                               throw new InvalidOperationException($"Failed to create instance of type {typeof(T)}");
        
        return configuration
            .GetSection(settingsInstance.SectionName)
            .Get<T>() ?? throw new InvalidOperationException($"{settingsInstance.SectionName} section not found");
    }
}