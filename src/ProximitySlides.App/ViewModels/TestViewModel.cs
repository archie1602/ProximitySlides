using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public partial class TestViewModel : ObservableObject
{
    private readonly WebserverLite _server;

    public TestViewModel()
    {
        var settings = new WebserverSettings("127.0.0.1", 9000);
        _server = new WebserverLite(settings, DefaultRoute);
        
        _server.Routes.PreAuthentication.Content.BaseDirectory = FileSystem.Current.AppDataDirectory;

        _server.Routes.PreAuthentication.Content.Add(
            "/pdfjs/",
            true);
    }

    private static async Task DefaultRoute(HttpContextBase ctx) =>
        await ctx.Response.Send($"Hello from default route: {ctx.Request.Url.RawWithQuery}");

    [ObservableProperty] private string _pathToPdf = null!;

    [RelayCommand]
    private async Task OnAppearing()
    {
        PathToPdf = $"http://127.0.0.1:9000/pdfjs/index.html?file=./assets/Introduction_to_Hadoop_slides.pdf";

        _server.Start();

        //for (var i = 2; i <= 20; i++)
        //{
        //    PathToPdf = $"http://127.0.0.1:9000/pdfjs/index.html?file=./assets/Introduction_to_Hadoop_slides.pdf&page={i}";

        //    await Task.Delay(TimeSpan.FromSeconds(2));
        //}
    }

    [RelayCommand]
    private void OnDisappearing()
    {
        _server.Stop();
    }
}