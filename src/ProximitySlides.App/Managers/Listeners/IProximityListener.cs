using ProximitySlides.App.Models;

namespace ProximitySlides.App.Managers.Listeners;

public interface IProximityListener
{
    void StartListenConcreteSpeaker(
        bool isExtended,
        string appId,
        SpeakerIdentifier speakerIdentifier,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StartListenAllSpeakers(
        bool isExtended,
        string appId,
        Action<BlePackageMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StopListen();
}
