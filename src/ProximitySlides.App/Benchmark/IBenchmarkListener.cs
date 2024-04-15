using ProximitySlides.App.Managers;

namespace ProximitySlides.App.Benchmark;

public interface IBenchmarkListener
{
    void StartListen(
        bool isExtended,
        string appId,
        Action<BenchmarkListenerMessage>? listenResultCallback,
        Action<ListenFailed>? listenFailedCallback);

    void StopListen();
}
