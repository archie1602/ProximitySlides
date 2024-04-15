using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

using ProximitySlides.App.Benchmark;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Pages;
using ProximitySlides.App.ViewModels;

namespace ProximitySlides.App.Configuration;

public static class Bootstraps
{
    [UsedImplicitly]
    public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddPages()
            .AddViewModels();
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SpeakerViewModel>();
        services.AddSingleton<ListenerViewModel>();
        services.AddSingleton<PresentationViewModel>();
        services.AddSingleton<BrowserViewModel>();
        services.AddSingleton<TestViewModel>();

        services.AddSingleton<BenchmarkSpeakerViewModel>();
        services.AddSingleton<BenchmarkListenerViewModel>();

        services.AddSingleton<ISlideListener, SlideListener>();
        services.AddSingleton<IBenchmarkListener, BenchmarkListener>();

        return services;
    }

    private static IServiceCollection AddPages(this IServiceCollection services)
    {
        // main tabs of the app
        services.AddTransient<MainPage>();

        // pages that are navigated to
        services.AddTransient<SpeakerPage>();
        services.AddTransient<ListenerPage>();
        services.AddTransient<PresentationPage>();
        services.AddTransient<BrowserPage>();
        services.AddTransient<TestPage>();

        services.AddTransient<BenchmarkSpeakerPage>();
        services.AddTransient<BenchmarkListenerPage>();

        return services;
    }
}
