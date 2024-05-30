using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace ProximitySlides.App.Applications;

[UsedImplicitly]
public class AppSettings : IBaseSettings
{
    public string SectionName => "App";

    public string AppAdvertiserId { get; set; } = null!;

    public string FileSharingUrlPrefix { get; set; } = null!;
}
