namespace Distrings;

public readonly record struct HashRangeBoundary(ulong Value, bool IsInclusive)
{
    public static HashRangeBoundary Inclusive(ulong value)
        => new(value, IsInclusive: true);

    public static HashRangeBoundary Exclusive(ulong value)
        => new(value, IsInclusive: false);

    public static bool operator <(HashRangeBoundary a, HashRangeBoundary b)
    {
        return a.Value < b.Value
               || a.Value == b.Value && a.IsInclusive && !b.IsInclusive;
    }

    public static bool operator >(HashRangeBoundary a, HashRangeBoundary b)
    {
        return a.Value > b.Value
               || a.Value == b.Value && a.IsInclusive && !b.IsInclusive;
    }

    public static bool operator <=(HashRangeBoundary a, HashRangeBoundary b)
    {
        return a < b || a == b;
    }

    public static bool operator >=(HashRangeBoundary a, HashRangeBoundary b)
    {
        return a > b || a == b;
    }

    public static bool operator <=(ulong value, HashRangeBoundary boundary)
    {
        var boundaryValue = boundary.Value;

        return value < boundaryValue || boundary.IsInclusive && boundaryValue == value;
    }

    public static bool operator >=(ulong value, HashRangeBoundary boundary)
    {
        var boundaryValue = boundary.Value;

        return value > boundaryValue || boundary.IsInclusive && boundaryValue == value;
    }
}