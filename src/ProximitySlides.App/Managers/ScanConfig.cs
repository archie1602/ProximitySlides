using Android.Bluetooth.LE;

namespace ProximitySlides.App.Managers;

public record ScanConfig(ScanMode Mode, string ServiceDataUuid);