namespace ProximitySlides.Core.Managers.Scanners;

public class BleScanResult
{
    public BleScanRecord ScanRecord { get; set; }
    
    public byte[] GetServiceData(Guid uuid)
    {
        return null;
    }
}