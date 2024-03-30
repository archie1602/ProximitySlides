using System.Text;
using ProximitySlides.Core;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Common;
using ProximitySlides.Core.Managers.Advertisers.Extended;

namespace ProximitySlides.App.Managers.Speakers;

public class BleSpeaker : IProximitySender
{
    private static readonly TimeSpan BroadcastDelayBetweenPackages = TimeSpan.FromMilliseconds(100);
    private const int BlePacketPayloadLength = 23;
    private const int BlePacketSenderIdLength = 2;
    private const string BaseUuid = "0000{0}-0000-1000-8000-00805F9B34FB";

    private static readonly Random Random = new();

    private readonly IBleAdvertiser _bleAdvertiser;
    private readonly IBleExtendedAdvertiser _bleExtendedAdvertiser;

    public BleSpeaker(
        IBleAdvertiser bleAdvertiser,
        IBleExtendedAdvertiser bleExtendedAdvertiser)
    {
        _bleAdvertiser = bleAdvertiser;
        _bleExtendedAdvertiser = bleExtendedAdvertiser;
    }

    private void OnStartSuccess(BleAdvertiseSettings? bleAdvertiseSettings)
    {
        // TODO:
    }

    private void OnStartFailure(BleAdvertiseFailure errorCode)
    {
        // TODO:
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

    public SpeakerIdentifier GenerateSenderIdentifier()
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

        return new SpeakerIdentifier(senderId);
    }

    public async Task SendMessage(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var appUuid = GetSenderUuid(appId);
        var senderIdBytes = Encoding.ASCII.GetBytes(speakerIdentifier.SpeakerId);

        var advSettings = new AdvertisementSettings(
            Mode: BleAdvertiseMode.LowLatency,
            TxPowerLevel: BleAdvertiseTx.PowerHigh,
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

            var advOptions = new AdvertisementOptions(
                Settings: advSettings,
                Data: new AdvertisementCommonData(
                    IncludeDeviceName: false,
                    IncludeTxPowerLevel: false,
                    ServicesData: new List<ServiceData> { new(appUuid, packageToSend) }));

            _bleAdvertiser.StartAdvertising(
                options: advOptions,
                startSuccessCallback: OnStartSuccess,
                startFailureCallback: OnStartFailure);

            await Task.Delay(BroadcastDelayBetweenPackages, cancellationToken);

            _bleAdvertiser.StopAdvertising();
        }
    }

    public async Task SendExtendedMessage(
        string appId,
        SpeakerIdentifier speakerIdentifier,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var appUuid = GetSenderUuid(appId);
        var senderIdBytes = Encoding.ASCII.GetBytes(speakerIdentifier.SpeakerId);

        var advSettings = new ExtendedAdvertisementSettings(
            Interval: ExtendedAdvertisementInterval.IntervalHigh,
            TxPowerLevel: BleExtendedAdvertiseTx.High,
            PrimaryPhy: AdvertisementBluetoothPhy.Le1m,
            SecondaryPhy: AdvertisementBluetoothPhy.Le2m,
            IsConnectable: false);

        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page)) = 31 - 8 = 23 bytes (payload)

        var maxAdvertisingDataLength = _bleExtendedAdvertiser.GetMaxAdvertisingDataLength();
        var blePacketPayloadLength = maxAdvertisingDataLength - (1 + 2 + 2 + 1 + 1);

        var totalPackages = (int)Math.Ceiling((double)data.Length / blePacketPayloadLength);

        var senderLength = senderIdBytes.Length;

        for (var i = 0; i < totalPackages; i++)
        {
            var startIndex = i * blePacketPayloadLength;
            int endIndex;

            if (i == totalPackages - 1)
            {
                endIndex = data.Length;
            }
            else
            {
                endIndex = startIndex + blePacketPayloadLength;
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

            var advOptions = new AdvertisementExtendedOptions(
                Settings: advSettings,
                Data: new AdvertisementCommonData(
                    IncludeDeviceName: false,
                    IncludeTxPowerLevel: false,
                    ServicesData: new List<ServiceData> { new(appUuid, packageToSend) }));

            _bleExtendedAdvertiser.StartAdvertising(
                options: advOptions,
                startSuccessCallback: OnStartSuccess,
                startFailureCallback: OnStartFailure);

            await Task.Delay(BroadcastDelayBetweenPackages, cancellationToken);

            _bleExtendedAdvertiser.StopAdvertising();
        }
    }
}
