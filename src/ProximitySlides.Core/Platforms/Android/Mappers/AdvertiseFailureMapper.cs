﻿using Android.Bluetooth.LE;
using ProximitySlides.Core.Managers.Advertisers;

namespace ProximitySlides.Core.Platforms.Android.Mappers;

public static class AdvertiseFailureMapper
{
    public static BleAdvertiseFailure Map(AdvertiseFailure advertiseFailure)
    {
        return BleAdvertiseFailure.AlreadyStarted;

        // return advertiseFailure switch
        // {
        //     BleAdvertiseMode.LowPower => AdvertiseMode.LowPower, 
        //     BleAdvertiseMode.Balanced => AdvertiseMode.Balanced, 
        //     BleAdvertiseMode.LowLatency => AdvertiseMode.LowLatency,
        //     _ => throw new ArgumentOutOfRangeException(nameof(advertiseMode))
        // };
    }
}