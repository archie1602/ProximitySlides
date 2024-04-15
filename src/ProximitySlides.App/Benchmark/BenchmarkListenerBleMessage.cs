namespace ProximitySlides.App.Benchmark;

public class BenchmarkListenerBleMessage
{
    public required int CurrentPackage { get; set; }

    public required int TotalPackages { get; set; }

    public required byte[] Payload { get; set; }

    public required int Rssi { get; set; }

    public required DateTime SentAt { get; set; }

    public required DateTime ReceivedAt { get; set; }
}
