using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Configuration;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.Core;

namespace ProximitySlides.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.AddAppSettings();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddApp(builder.Configuration);
        builder.Services.AddCore();

        builder.Services.AddSingleton<IProximitySender, BleSpeaker>();
        builder.Services.AddSingleton<IProximityListener, BleListener>();

        return builder.Build();
    }

    private static void AddAppSettings(this MauiAppBuilder builder)
    {
        builder.AddJsonSettings("appsettings.json");

#if DEBUG
        builder.AddJsonSettings("appsettings.development.json");
#endif

#if !DEBUG
        builder.AddJsonSettings("appsettings.production.json");
#endif
    }

    private static void AddJsonSettings(this MauiAppBuilder builder, string fileName)
    {
        using var stream = Assembly
                               .GetExecutingAssembly()
                               .GetManifestResourceStream("ProximitySlides.App.appsettings.json") ??
                           throw new InvalidOperationException("Couldn't found appsettings configuration file");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);
    }
}