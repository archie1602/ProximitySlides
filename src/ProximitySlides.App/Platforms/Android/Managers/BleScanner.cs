using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using ProximitySlides.App.Exceptions;
using ProximitySlides.App.Managers;

namespace ProximitySlides.App.Platforms.Android.Managers;

public class BleScanner : ScanCallback, IBleScanner
{
    private Action<ScanCallbackType, ScanResult?>? _onScanResultHandler;
    private Action<ScanFailure>? _onScanFailedHandler;
    
    public BleScanner()
    {
        // _appContext = appContext;
        //
        // if (_appContext?.ApplicationContext?.GetSystemService(Context.BluetoothService) is not BluetoothManager bluetoothManager)
        // {
        //     throw new BluetoothServiceIsNotAvailable("Bluetooth service is not available");
        // }
        //
        // Native = bluetoothManager;
    }
    
    public bool IsScanning { get; private set; }

    public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
    {
        base.OnScanResult(callbackType, result);
        _onScanResultHandler?.Invoke(callbackType, result);
    }

    public override void OnScanFailed(ScanFailure errorCode)
    {
        base.OnScanFailed(errorCode);
        _onScanFailedHandler?.Invoke(errorCode);
    }

    public void StartScan(
        ScanConfig scanConfig,
        Action<ScanCallbackType, ScanResult?>? scanResultCallback = null,
        Action<ScanFailure>? scanFailedCallback = null)
    {
        if (IsScanning)
        {
            throw new InvalidOperationException("There is already an active scan");
        }

        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var bleAdapter = bleManager?.Adapter;
        var bleScanner = bleAdapter?.BluetoothLeScanner;

        if (bleScanner is null)
        {
            throw new BleNotAvailableException("BluetoothLeScanner is not available");
        }

        if (bleAdapter is { IsEnabled: false })
        {
            throw new BluetoothAdapterIsDisabled("Bluetooth adapter is disabled");
        }
        
        var serviceUuid = ParcelUuid.FromString(scanConfig.ServiceDataUuid);
        
        var settings = new ScanSettings.Builder()
            .SetScanMode(scanConfig.Mode)
            ?.Build();

        if (settings is null)
        {
            throw new BleException("Unable to create scan settings");
        }
        
        var filter = new ScanFilter.Builder()
            .SetServiceData(
                serviceDataUuid: serviceUuid,
                serviceData: null)
            ?.Build();

        if (filter is null)
        {
            throw new BleException("Unable to create scan filter");
        }
        
        _onScanResultHandler = scanResultCallback;
        _onScanFailedHandler = scanFailedCallback;
        
        bleScanner.StartScan(
            filters: new List<ScanFilter> { filter },
            settings: settings,
            callback: this);
        
        IsScanning = true;
    }

    public void StopScan()
    {
        if (!IsScanning)
        {
            return;
        }
        
        var bleManager = Platform.CurrentActivity?.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var bleAdapter = bleManager?.Adapter;
        var bleScanner = bleAdapter?.BluetoothLeScanner;
        
        if (bleScanner is null)
        {
            throw new BleNotAvailableException("BluetoothLeScanner is not available");
        }
        
        bleScanner.StopScan(this);
        
        _onScanResultHandler = null;
        _onScanFailedHandler = null;
        
        IsScanning = false;
    }
}