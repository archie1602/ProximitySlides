using ProximitySlides.App.Managers;

namespace ProximitySlides.App.Benchmark;

public class BenchmarkBlePackageComparator : Comparer<BenchmarkListenerBleMessage>
{
    public override int Compare(BenchmarkListenerBleMessage? x, BenchmarkListenerBleMessage? y)
    {
        // TODO: fix nullability
        return x.CurrentPackage.CompareTo(y.CurrentPackage);
    }
}
