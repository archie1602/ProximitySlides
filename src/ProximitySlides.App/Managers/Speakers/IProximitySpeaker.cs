namespace ProximitySlides.App.Managers.Speakers;

public interface IProximitySpeaker
{
    Task SendMessage(
        string appId,
        byte[] data,
        CancellationToken cancellationToken);

    Task SendMessage(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        byte[] data,
        CancellationToken cancellationToken);

    Task SendExtendedMessage(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        byte[] data,
        CancellationToken cancellationToken);

    SpeakerIdentifier GenerateSenderIdentifier();
}
