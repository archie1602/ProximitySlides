using ProximitySlides.App.Models;

namespace ProximitySlides.App.Managers.Listeners;

public interface IProximityListener
{
    void StartListenSpeaker(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StartListenAllSpeakers(
        string appId,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StopListen();
}