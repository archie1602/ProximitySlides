using ProximitySlides.App.Models;

namespace ProximitySlides.App.Managers.Listeners;

public interface IProximityListener
{
    void StartListenSpeaker(
        string appId,
        SenderIdentifier senderIdentifier,
        Action<BlePackageModel>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StartListenAllSpeakers(
        string appId,
        Action<BlePackageModel>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StopListen();
}