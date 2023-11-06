using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class PdfViewerWebServerSettings
{
    public static string SectionName => $"{SettingsHelper.GetSectionName<PresentationSettings>()}:PdfViewerWebServer";

    public string Hostname { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 15678;
}