using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Pages;
using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Configuration;

public static class Bootstraps
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration)
    {
        //services.Configure<ListenerSettings>(_ => configuration.GetSection(ListenerSettings.SectionName).Get<ListenerSettings>());

        return services
            .AddPages()
            .AddViewModels();
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SpeakerViewModel>();
        services.AddSingleton<ListenerViewModel>();
        services.AddSingleton<ListenerDetailsViewModel>();
        services.AddSingleton<TestViewModel>();

        services.AddSingleton<ISlideListener, SlideListener>();
        
        return services;
    }

    private static IServiceCollection AddPages(this IServiceCollection services)
    {
        // main tabs of the app
        services.AddTransient<MainPage>();
        
        // pages that are navigated to
        services.AddTransient<SpeakerPage>();
        services.AddTransient<ListenerPage>();
        services.AddTransient<ListenerDetailsPage>();
        services.AddTransient<TestPage>();
        
        return services;
    }
}