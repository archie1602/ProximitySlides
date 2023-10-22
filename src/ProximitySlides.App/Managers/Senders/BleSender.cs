using System.Text;
using Android.Bluetooth.LE;

namespace ProximitySlides.App.Managers.Senders;

public class BleSender : IProximitySender
{
    private static readonly TimeSpan BroadcastDelayBetweenPackages = TimeSpan.FromMilliseconds(100);
    private const int BlePacketPayloadLength = 23;
    private const int BlePacketSenderIdLength = 2;
    private const string BaseUuid = "0000{0}-0000-1000-8000-00805F9B34FB";
    
    private static readonly Random Random = new();
    
    private readonly IBleAdvertiser _bleAdvertiser;

    public BleSender(IBleAdvertiser bleAdvertiser)
    {
        _bleAdvertiser = bleAdvertiser;
    }
    
    private void OnStartSuccess(AdvertiseSettings? settingsInEffect)
    {
        // TODO
    }
    
    private void OnStartFailure(AdvertiseFailure errorCode)
    {
        // TODO
    }

    private static string GetSenderUuid(string appId)
    {
        // TODO: добавить проверку чисел на шестнадцатеричность
        if (appId.Length != 4)
        {
            throw new ArgumentException("AppId must consist of two hexadecimal numbers");
        }
        
        return string.Format(BaseUuid, appId);
    }

    public SenderIdentifier GenerateSenderIdentifier()
    {
        var charsetBuilder = new StringBuilder();

        // [0-9]
        for (var i = 48; i <= 57; i++)
        {
            charsetBuilder.Append((char)i);
        }
        
        // [A-Z]
        for (var i = 65; i <= 90; i++)
        {
            charsetBuilder.Append((char)i);
        }
        
        // [a-z]
        for (var i = 97; i <= 122; i++)
        {
            charsetBuilder.Append((char)i);
        }

        var chars = charsetBuilder.ToString();
        
        var senderId = new string(Enumerable
            .Repeat(chars, BlePacketSenderIdLength)
            .Select(s => s[Random.Next(s.Length)])
            .ToArray());

        return new SenderIdentifier(senderId);
    }

    public async Task SendMessage(
        string appId,
        SenderIdentifier senderIdentifier,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var appUuid = GetSenderUuid(appId);
        var senderIdBytes = Encoding.ASCII.GetBytes(senderIdentifier.SenderId);
        
        var advSettings = new AdvertisementSettings(
            Mode: AdvertiseMode.LowLatency,
            TxPowerLevel: AdvertiseTx.PowerHigh,
            IsConnectable: false);

        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page)) = 31 - 8 = 23 bytes (payload)
        
        var totalPackages = (int)Math.Ceiling((double)data.Length / BlePacketPayloadLength);

        var senderLength = senderIdBytes.Length;
        
        for (var i = 0; i < totalPackages; i++)
        {
            var startIndex = i * BlePacketPayloadLength;
            int endIndex;

            if (i == totalPackages - 1)
            {
                endIndex = data.Length;
            }
            else
            {
                endIndex = startIndex + BlePacketPayloadLength;
            }

            var packageToSend = new byte[senderLength + 2 + endIndex - startIndex];
            
            // copy sender id: 'AB' for example - 2 bytes
            senderIdBytes.CopyTo(packageToSend, 0);
            
            // copy page # - 1 byte
            packageToSend[senderLength] = (byte)i;
            
            // copy total # of pages - 1 byte
            packageToSend[senderLength + 1] = (byte)totalPackages;
            
            // copy other payload - 23 bytes

            var k = senderLength + 2;

            for (var j = startIndex; j < endIndex; j++)
            {
                packageToSend[k++] = data[j];
            }
            
            var avdOptions = new AdvertisementOptions(
                Settings: advSettings,
                Data: new AdvertisementData(
                    IncludeDeviceName: false,
                    IncludeTxPowerLevel: false,
                    ServicesData: new List<ServiceData> { new(appUuid, packageToSend) }));
            
            _bleAdvertiser.StartAdvertising(
                options: avdOptions,
                startSuccessCallback: OnStartSuccess,
                startFailureCallback: OnStartFailure);
            
            await Task.Delay(BroadcastDelayBetweenPackages, cancellationToken);

            _bleAdvertiser.StopAdvertising();
        }
    }
}