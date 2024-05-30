namespace ProximitySlides.Core.Exceptions;

public class BluetoothAdapterIsDisabled : ApplicationException
{
    public BluetoothAdapterIsDisabled(string message) : base(message)
    {
    }
}