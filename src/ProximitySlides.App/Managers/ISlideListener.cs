namespace ProximitySlides.App.Managers;

public interface ISlideListener
{
    void StartListenSlides(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Func<SlideDto, Task>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);
    
    void StopListen();
}