namespace ProximitySlides.App.Exceptions;

public class BluetoothServiceIsNotAvailable : ApplicationException
{
    public BluetoothServiceIsNotAvailable(string message) : base(message)
    {
    }
}