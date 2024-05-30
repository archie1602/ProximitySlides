namespace ProximitySlides.Core.Managers.Scanners;

public record ScanConfig(
    bool IsExtended,
    BleScanMode Mode,
    string ServiceDataUuid);