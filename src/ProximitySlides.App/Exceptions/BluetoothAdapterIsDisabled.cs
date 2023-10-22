namespace ProximitySlides.App.Exceptions;

public class BluetoothAdapterIsDisabled : ApplicationException
{
    public BluetoothAdapterIsDisabled(string message) : base(message)
    {
    }
}