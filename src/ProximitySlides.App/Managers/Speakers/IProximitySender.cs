namespace ProximitySlides.App.Managers.Speakers;

public interface IProximitySender
{
    Task SendMessage(string appId, SpeakerIdentifier speakerIdentifier, byte[] data, CancellationToken cancellationToken);
    SpeakerIdentifier GenerateSenderIdentifier();
}