using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using ProximitySlides.App.Exceptions;
using ProximitySlides.App.Managers;

namespace ProximitySlides.App.Platforms.Android.Managers;

public class BleAdvertiser : AdvertiseCallback, IBleAdvertiser
{
    private Action<AdvertiseSettings?>? _onStartSuccessHandler;
    private Action<AdvertiseFailure>? _onStartFailureHandler;

    public BleAdvertiser()
    {
        // if (_appContext?.ApplicationContext?.GetSystemService(Context.BluetoothService) is not BluetoothManager
        //     bluetoothManager)
        // {
        //     throw new BluetoothServiceIsNotAvailable("Bluetooth service is not available");
        // }
        //
        // Native = bluetoothManager;
    }

    public bool IsAdvertising { get; private set; }

    public override void OnStartSuccess(AdvertiseSettings? settingsInEffect)
    {
        base.OnStartSuccess(settingsInEffect);
        _onStartSuccessHandler?.Invoke(settingsInEffect);
    }

    public override void OnStartFailure(AdvertiseFailure errorCode)
    {
        base.OnStartFailure(errorCode);
        _onStartFailureHandler?.Invoke(errorCode);
    }

    public void StartAdvertising(
        AdvertisementOptions options,
        Action<AdvertiseSettings?>? startSuccessCallback = null,
        Action<AdvertiseFailure>? startFailureCallback = null)
    {
        if (IsAdvertising)
        {
            throw new InvalidOperationException("Advertisement is already running");
        }

        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var bleAdvertiser = bleManager?.Adapter?.BluetoothLeAdvertiser;

        if (bleAdvertiser is null)
        {
            throw new BleNotAvailableException("BluetoothLeAdvertiser is not available");
        }

        var settings = new AdvertiseSettings.Builder()
            .SetAdvertiseMode(options.Settings.Mode)
            ?.SetTxPowerLevel(options.Settings.TxPowerLevel)
            ?.SetConnectable(options.Settings.IsConnectable)
            ?.Build();

        if (settings is null)
        {
            throw new BleException("Unable to create advertisement settings");
        }
        
        var dataBuilder = new AdvertiseData.Builder()
            .SetIncludeDeviceName(options.Data.IncludeDeviceName)
            ?.SetIncludeTxPowerLevel(options.Data.IncludeTxPowerLevel);

        foreach (var sd in options.Data.ServicesData)
        {
            dataBuilder?.AddServiceData(
                serviceDataUuid: ParcelUuid.FromString(sd.Uuid),
                serviceData: sd.Data);
        }

        var data = dataBuilder?.Build();

        if (data is null)
        {
            throw new BleException("Unable to create advertisement data");
        }
        
        bleAdvertiser.StartAdvertising(
            settings: settings,
            advertiseData: data,
            callback: this);

        _onStartSuccessHandler = startSuccessCallback;
        _onStartFailureHandler = startFailureCallback;
        IsAdvertising = true;
    }

    public void StopAdvertising()
    {
        if (!IsAdvertising)
        {
            return;
        }
        
        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var bleAdvertiser = bleManager?.Adapter?.BluetoothLeAdvertiser;

        if (bleAdvertiser is null)
        {
            throw new BleNotAvailableException("BluetoothLeAdvertiser is not available");
        }

        bleAdvertiser.StopAdvertising(this);
        
        _onStartSuccessHandler = null;
        _onStartFailureHandler = null;
        
        IsAdvertising = false;
    }
}