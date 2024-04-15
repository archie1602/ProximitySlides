namespace ProximitySlides.App.Benchmark;

public record BenchmarkListenerMessage(
    byte[] Message,
    TimeSpan TotalTransmissionTime,
    List<int> PackagesRssi);
