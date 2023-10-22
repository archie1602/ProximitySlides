namespace ProximitySlides.App.Managers;

public interface ISlideListener
{
    void StartListenSlides(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Action<SlideDto>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);
    
    void StopListen();
}