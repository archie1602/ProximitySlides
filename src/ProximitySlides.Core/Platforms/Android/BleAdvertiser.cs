using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using ProximitySlides.Core.Exceptions;
using ProximitySlides.Core.Managers;
using ProximitySlides.Core.Managers.Advertisers;
using ProximitySlides.Core.Platforms.Android.Mappers;

namespace ProximitySlides.Core.Platforms.Android;

public class BleAdvertiser : AdvertiseCallback, IBleAdvertiser
{
    private Action<BleAdvertiseSettings?>? _onStartSuccessHandler;
    private Action<BleAdvertiseFailure>? _onStartFailureHandler;

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

        var bleSettingsInEffect = AdvertiseSettingsMapper.Map(settingsInEffect);
        _onStartSuccessHandler?.Invoke(bleSettingsInEffect);
    }

    public override void OnStartFailure(AdvertiseFailure errorCode)
    {
        base.OnStartFailure(errorCode);
        
        var bleErrorCode = AdvertiseFailureMapper.Map(errorCode);
        _onStartFailureHandler?.Invoke(bleErrorCode);
    }

    public void StartAdvertising(
        AdvertisementOptions options,
        Action<BleAdvertiseSettings?>? startSuccessCallback = null,
        Action<BleAdvertiseFailure>? startFailureCallback = null)
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

        var advertiseMode = AdvertiseModeMapper.Map(options.Settings.Mode);
        var advertiseTxPowerLevel = AdvertiseTxMapper.Map(options.Settings.TxPowerLevel);

        var settings = new AdvertiseSettings.Builder()
            .SetAdvertiseMode(advertiseMode)
            ?.SetTxPowerLevel(advertiseTxPowerLevel)
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