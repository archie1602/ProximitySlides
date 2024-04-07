namespace ProximitySlides.App.Managers;

public interface ISlideListener
{
    void StartListenSlides(
        bool isExtended,
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Func<SlideMessage, Task>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StopListen();
}
