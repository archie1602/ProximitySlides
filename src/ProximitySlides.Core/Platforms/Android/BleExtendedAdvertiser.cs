using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using ProximitySlides.Core.Exceptions;
using ProximitySlides.Core.Managers.Advertisers;
using ProximitySlides.Core.Platforms.Android.Mappers;
using BluetoothPhy = Android.Bluetooth.BluetoothPhy;

namespace ProximitySlides.Core.Platforms.Android;

public class BleExtendedAdvertiser : AdvertisingSetCallback, IBleAdvertiser
{
    private Action<BleAdvertiseSettings?>? _onStartSuccessHandler;
    private Action<BleAdvertiseFailure>? _onStartFailureHandler;

    public bool IsAdvertising { get; private set; }
    
    public override void OnAdvertisingSetStarted(
        AdvertisingSet? advertisingSet,
        int txPower,
        AdvertiseResult status)
    {
        base.OnAdvertisingSetStarted(advertisingSet, txPower, status);
        
        _onStartSuccessHandler?.Invoke(new BleAdvertiseSettings());
    }

    public override void OnAdvertisingDataSet(AdvertisingSet? advertisingSet, AdvertiseResult status)
    {
        base.OnAdvertisingDataSet(advertisingSet, status);
    }

    public override void OnScanResponseDataSet(AdvertisingSet? advertisingSet, AdvertiseResult status)
    {
        base.OnScanResponseDataSet(advertisingSet, status);
    }

    public override void OnAdvertisingSetStopped(AdvertisingSet? advertisingSet)
    {
        base.OnAdvertisingSetStopped(advertisingSet);
        
        _onStartFailureHandler?.Invoke(BleAdvertiseFailure.AlreadyStarted);
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

        var advertiseMode = AdvertiseModeMapper.Map(options.Settings.Mode);
        var advertiseTxPowerLevel = AdvertiseTxMapper.Map(options.Settings.TxPowerLevel);

        var parameters = new AdvertisingSetParameters.Builder()
            .SetLegacyMode(true)
            // .SetConnectable(true)
            ?.SetInterval(AdvertisingSetParameters.IntervalHigh)
            ?.SetTxPowerLevel(AdvertiseTxPower.Medium)
            ?.Build();

        if (parameters is null)
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

        bleAdvertiser.StartAdvertisingSet(
            parameters: parameters,
            advertiseData: data,
            scanResponse: null,
            periodicParameters: null,
            periodicData: null,
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

        bleAdvertiser.StopAdvertisingSet(this);
        
        _onStartSuccessHandler = null;
        _onStartFailureHandler = null;
        
        IsAdvertising = false;
    }
}