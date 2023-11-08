﻿using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class AppSettings : IBaseSettings
{
    public string SectionName => "App";
    
    public string AppAdvertiserId { get; set; } = null!;
    public PdfViewerWebServerSettings PdfViewerWebServer { get; set; } = null!;
}