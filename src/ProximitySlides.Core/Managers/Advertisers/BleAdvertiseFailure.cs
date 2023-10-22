namespace ProximitySlides.Core.Managers.Advertisers;

public enum BleAdvertiseFailure
{
    DataTooLarge, 
    TooManyAdvertisers,
    AlreadyStarted,
    InternalError,
    FeatureUnsupported,
}