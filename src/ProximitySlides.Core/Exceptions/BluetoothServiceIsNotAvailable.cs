namespace ProximitySlides.Core.Exceptions;

public class BluetoothServiceIsNotAvailable : ApplicationException
{
    public BluetoothServiceIsNotAvailable(string message) : base(message)
    {
    }
}