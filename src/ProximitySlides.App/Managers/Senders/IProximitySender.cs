namespace ProximitySlides.App.Managers.Senders;

public interface IProximitySender
{
    Task SendMessage(string appId, SenderIdentifier senderIdentifier, byte[] data, CancellationToken cancellationToken);
    SenderIdentifier GenerateSenderIdentifier();
}