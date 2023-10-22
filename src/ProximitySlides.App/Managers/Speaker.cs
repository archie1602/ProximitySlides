namespace ProximitySlides.App.Managers;

public class Speaker
{
    public required string SpeakerId { get; init; }
    public int CountReceivedPackages { get; set; }
    public required int MaxPackages { get; set; }
    public required DateTime LastActivityTime { get; set; }

    private bool Equals(Speaker other)
    {
        return SpeakerId == other.SpeakerId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((Speaker)obj);
    }

    public override int GetHashCode()
    {
        return SpeakerId.GetHashCode();
    }

    public static bool operator ==(Speaker s1, Speaker s2)
    {
        return s1.SpeakerId == s2.SpeakerId;
    }

    public static bool operator !=(Speaker s1, Speaker s2)
    {
        return !(s1 == s2);
    }
}