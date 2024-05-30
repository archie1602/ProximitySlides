namespace ProximitySlides.Core.Managers.Scanners;

public enum BleScanFailure
{ 
    AlreadyStarted,
    ApplicationRegistrationFailed,
    InternalError,
    FeatureUnsupported,
    OutOfHardwareResources,
    ScanningTooFrequently
}