using System.Diagnostics.CodeAnalysis;

using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using ProximitySlides.Core.Exceptions;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Extended;
using ProximitySlides.Core.Platforms.Android.Ble.Common.Mappers;
using ProximitySlides.Core.Platforms.Android.Ble.Extended.Mappers;

namespace ProximitySlides.Core.Platforms.Android.Ble.Extended;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class BleExtendedAdvertiser : AdvertisingSetCallback, IBleExtendedAdvertiser
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

        if (status == AdvertiseResult.Success)
        {
            _onStartSuccessHandler?.Invoke(new BleAdvertiseSettings());
        }
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

    public int GetMaxAdvertisingDataLength()
    {
        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        return bleManager?.Adapter?.LeMaximumAdvertisingDataLength ?? 31;
    }

    public void StartAdvertising(
        AdvertisementExtendedOptions options,
        Action<BleAdvertiseSettings?>? startSuccessCallback = null,
        Action<BleAdvertiseFailure>? startFailureCallback = null)
    {
        if (IsAdvertising)
        {
            throw new InvalidOperationException("Advertisement is already running");
        }

        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var bleAdvertiser = bleManager?.Adapter?.BluetoothLeAdvertiser;

        var advertiseTxPowerLevel = ExtendedAdvertiseTxMapper.Map(options.Settings.TxPowerLevel);
        var primaryPhy = BluetoothPhyMapper.Map(options.Settings.PrimaryPhy);
        var secondaryPhy = BluetoothPhyMapper.Map(options.Settings.SecondaryPhy);

        var parameters = new AdvertisingSetParameters.Builder()
            .SetLegacyMode(false)
            ?.SetConnectable(options.Settings.IsConnectable)
            ?.SetInterval(options.Settings.Interval)
            ?.SetTxPowerLevel(advertiseTxPowerLevel)
            ?.SetPrimaryPhy(primaryPhy)
            ?.SetSecondaryPhy(secondaryPhy)
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

        bleAdvertiser?.StartAdvertisingSet(
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
