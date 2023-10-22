namespace ProximitySlides.App.Exceptions;

public class BleNotAvailableException : BleException
{
    public BleNotAvailableException(string message) : base(message)
    {
    }
}