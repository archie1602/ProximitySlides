using System.Reflection;
using CommunityToolkit.Maui;
using MetroLog.MicrosoftExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Configuration;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.Core;
using SkiaSharp;

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

        builder.Logging.AddTraceLogger(_ => { });
        builder.AddAppSettings();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddApp(builder.Configuration);
        builder.Services.AddCore();

        builder.Services.AddSingleton<IProximitySender, BleSpeaker>();
        builder.Services.AddSingleton<IProximityListener, BleListener>();

        builder.Services.AddSingleton<IFilePicker>(FilePicker.Default);

        var app = builder.Build();

        // Task.Run(async () =>
        // {
        //     var rootDir = "presentations";
        //     var pathToSlidesDir = Path.Combine(FileSystem.Current.AppDataDirectory, rootDir, "2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9", "slides");
        //     var pdfPath = Path.Combine(FileSystem.Current.AppDataDirectory, rootDir, "2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9", "Introduction_to_Hadoop.pdf");
        //
        //     using (var pdfStream = File.OpenRead(pdfPath))
        //     {
        //         var result = PDFtoImage.Conversion.ToImagesAsync(pdfStream);
        //
        //         var i = 1;
        //     
        //         await foreach (var pdfImage in result)
        //         {
        //             using (var imageStream =
        //                    File.Create(
        //                        Path.Combine(pathToSlidesDir, $"slide_{i++}.png")))
        //             {
        //                 pdfImage.Encode(imageStream, SKEncodedImageFormat.Png, 100);
        //             }
        //         }
        //     }
        // })
        // .GetAwaiter()
        // .GetResult();

        return app;
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