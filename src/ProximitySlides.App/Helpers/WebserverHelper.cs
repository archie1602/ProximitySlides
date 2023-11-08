using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.Helpers;

public static class WebserverHelper
{
    public static WebserverLite BuildPdfViewerWebserver(string hostname, int port)
    {
        var settings = new WebserverSettings(hostname, port);
        
        var server = new WebserverLite(
            settings,
            async ctx =>
                await ctx.Response.Send($"Hello from default route: {ctx.Request.Url.RawWithQuery}"));
        
        server.Routes.PreAuthentication.Content.BaseDirectory = FileSystem.Current.AppDataDirectory;

        return server;
    }
}