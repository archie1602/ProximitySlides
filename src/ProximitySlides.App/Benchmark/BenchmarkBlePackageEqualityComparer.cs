using ProximitySlides.App.Managers;

namespace ProximitySlides.App.Benchmark;

public sealed class BenchmarkBlePackageEqualityComparer : IEqualityComparer<BenchmarkListenerBleMessage>
{
    public bool Equals(BenchmarkListenerBleMessage? x, BenchmarkListenerBleMessage? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.CurrentPackage == y.CurrentPackage;
    }

    public int GetHashCode(BenchmarkListenerBleMessage obj)
    {
        return obj.CurrentPackage.GetHashCode();
    }
}
