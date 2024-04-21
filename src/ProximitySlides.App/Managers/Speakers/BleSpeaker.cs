using System.Text;

using ProximitySlides.App.Helpers;
using ProximitySlides.Core;
using ProximitySlides.Core.Managers.Advertisers.Classic;
using ProximitySlides.Core.Managers.Advertisers.Common;
using ProximitySlides.Core.Managers.Advertisers.Extended;

namespace ProximitySlides.App.Managers.Speakers;

public class BleSpeaker : IProximitySpeaker
{
    private static readonly TimeSpan BroadcastDelayBetweenPackages = TimeSpan.FromMilliseconds(100);
    private const int BlePacketPayloadLength = 25 - 8;
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

    private static byte[] GetCurrentTimestamp()
    {
        var dto = new DateTimeOffset(DateTime.UtcNow);
        var nowUnixEpoch = dto.ToUnixTimeMilliseconds();

        // return 8 bytes
        return BitConverter.GetBytes(nowUnixEpoch);
    }

    public async Task SendMessage(string appId, byte[] data, CancellationToken cancellationToken)
    {
        var appUuid = GetSenderUuid(appId);

        var advSettings = new AdvertisementSettings(
            Mode: AppParameters.BleAdvertiseMode,
            TxPowerLevel: AppParameters.BleAdvertiseTx,
            IsConnectable: false);

        // 31 - (2 (uuid) + 2 (uuid length and type) + 1 (total pages) + 1 (current page) + 8 (start timestamp)) = 31 - 6 - 8 = 17 bytes (payload)

        var totalPackages = (int)Math.Ceiling((double)data.Length / BlePacketPayloadLength);

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

            var packageToSend = new byte[2 + 8 + endIndex - startIndex];

            // copy package # - 1 byte
            packageToSend[0] = (byte)i;

            // copy total # of packages - 1 byte
            packageToSend[1] = (byte)totalPackages;

            // copy other payload - 25 bytes - 8 bytes = 17 bytes

            var k = 2 + 8;

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

            // copy 'send timestamp'
            var nowTimestampBytes = GetCurrentTimestamp();

            var l = 2;

            foreach (var tb in nowTimestampBytes)
            {
                packageToSend[l++] = tb;
            }

            _bleAdvertiser.StartAdvertising(
                options: advOptions,
                startSuccessCallback: OnStartSuccess,
                startFailureCallback: OnStartFailure);

            await Task.Delay(TimeSpan.FromMilliseconds(AppParameters.BroadcastDelayBetweenPackagesMs), cancellationToken);

            _bleAdvertiser.StopAdvertising();
        }
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

        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page) - 8 (timestamp)) = 31 - 8 - 8 = 15 bytes (payload)

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

            var packageToSend = new byte[senderLength + 2 + 8 + endIndex - startIndex];

            // copy sender id: 'AB' for example - 2 bytes
            senderIdBytes.CopyTo(packageToSend, 0);

            // copy package # - 1 byte
            packageToSend[senderLength] = (byte)i;

            // copy total # of packages - 1 byte
            packageToSend[senderLength + 1] = (byte)totalPackages;

            // copy other payload - 23 bytes - 8 bytes = 15 bytes

            var k = senderLength + 2 + 8;

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

            // copy 'send timestamp'
            var nowTimestampBytes = GetCurrentTimestamp();

            var l = senderLength + 2;

            for (var j = 0; j < nowTimestampBytes.Length; j++)
            {
                packageToSend[l++] = nowTimestampBytes[j];
            }

            _bleAdvertiser.StartAdvertising(
                options: advOptions,
                startSuccessCallback: OnStartSuccess,
                startFailureCallback: OnStartFailure);

            await Task.Delay(TimeSpan.FromMilliseconds(AppParameters.BroadcastDelayBetweenPackagesMs), cancellationToken);

            _bleAdvertiser.StopAdvertising();
        }
    }

    public async Task SendExtendedMessage(
        string appId,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var appUuid = GetSenderUuid(appId);

        var advSettings = new ExtendedAdvertisementSettings(
            Interval: AppParameters.ExtendedBleAdvertiseMode,
            TxPowerLevel: AppParameters.ExtendedBleAdvertiseTx,
            PrimaryPhy: AdvertisementBluetoothPhy.Le1m,
            SecondaryPhy: AdvertisementBluetoothPhy.Le2m,
            IsConnectable: false);

        // 255 - (2 (uuid) + 2 (???) + 1 (total packages) + 1 (current package) + 8) = 31 - 8 = 23 bytes (payload)

        var maxAdvertisingDataLength = 192; //_bleExtendedAdvertiser.GetMaxAdvertisingDataLength();
        var blePacketPayloadLength = maxAdvertisingDataLength - (2 + 2 + 1 + 1 + 8);

        var totalPackages = (int)Math.Ceiling((double)data.Length / blePacketPayloadLength);

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

            var packageToSend = new byte[2 + 8 + endIndex - startIndex];

            // copy package # - 1 byte
            packageToSend[0] = (byte)i;

            // copy total # of packages - 1 byte
            packageToSend[1] = (byte)totalPackages;

            // copy other payload - 23 bytes

            var k = 2 + 8;

            for (var j = startIndex; j < endIndex; j++)
            {
                packageToSend[k++] = data[j];
            }

            // copy 'send timestamp'
            var nowTimestampBytes = GetCurrentTimestamp();

            var l = 2;

            foreach (var t in nowTimestampBytes)
            {
                packageToSend[l++] = t;
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

            await Task.Delay(TimeSpan.FromMilliseconds(AppParameters.BroadcastDelayBetweenPackagesMs), cancellationToken);

            _bleExtendedAdvertiser.StopAdvertising();
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
            Interval: ExtendedAdvertisementInterval.IntervalLow,
            TxPowerLevel: BleExtendedAdvertiseTx.High,
            PrimaryPhy: AdvertisementBluetoothPhy.Le1m,
            SecondaryPhy: AdvertisementBluetoothPhy.Le2m,
            IsConnectable: false);

        // 31 - (2 + 2 (uuid) + 2 bytes (deviceId) + 1 (total pages) + 1 (current page)) = 31 - 8 = 23 bytes (payload)

        var maxAdvertisingDataLength = _bleExtendedAdvertiser.GetMaxAdvertisingDataLength();
        var blePacketPayloadLength = maxAdvertisingDataLength - (1 + 2 + 2 + 1 + 1 + 8);

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

            var packageToSend = new byte[senderLength + 2 + 8 + endIndex - startIndex];

            // copy sender id: 'AB' for example - 2 bytes
            senderIdBytes.CopyTo(packageToSend, 0);

            // copy package # - 1 byte
            packageToSend[senderLength] = (byte)i;

            // copy total # of packages - 1 byte
            packageToSend[senderLength + 1] = (byte)totalPackages;

            // copy other payload - 23 bytes

            var k = senderLength + 2 + 8;

            for (var j = startIndex; j < endIndex; j++)
            {
                packageToSend[k++] = data[j];
            }

            // copy 'send timestamp'
            var nowTimestampBytes = GetCurrentTimestamp();

            var l = senderLength + 2;

            for (var j = 0; j < nowTimestampBytes.Length; j++)
            {
                packageToSend[l++] = nowTimestampBytes[j];
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

            await Task.Delay(TimeSpan.FromMilliseconds(AppParameters.BroadcastDelayBetweenPackagesMs), cancellationToken);

            _bleExtendedAdvertiser.StopAdvertising();
        }
    }
}
