using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class PresentationSettings : IBaseSettings
{
    public string SectionName => "Presentation";

    public PdfViewerWebServerSettings PdfViewerWebServer { get; set; } = null!;
}