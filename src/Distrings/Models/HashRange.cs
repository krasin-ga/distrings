using System.Text.RegularExpressions;

namespace Distrings;

public readonly record struct HashRange : IComparable<HashRange>
{
    public HashRangeBoundary From { get; }
    public HashRangeBoundary To { get; }

    public HashRange(
        HashRangeBoundary from,
        HashRangeBoundary to)
    {
        if (from > to && !(from < to))
            throw new ArgumentOutOfRangeException(
                nameof(from),
                "From must be less than To. " +
                "If you need to loop around the ring, than use the LoopAround method.");

        if (from.Value == to.Value && (!from.IsInclusive || !to.IsInclusive))
            throw new ArgumentOutOfRangeException(
                nameof(from),
                "Both must be inclusive");

        From = from;
        To = to;
    }

    public static HashRange FullCoverage(IRingConfiguration ringConfiguration) => new(
        HashRangeBoundary.Inclusive(ulong.MinValue),
        HashRangeBoundary.Inclusive(ringConfiguration.MaxSlot));

    public static (HashRange Segment1, HashRange Segment2) LoopAround(
        IRingConfiguration config,
        HashRangeBoundary from,
        HashRangeBoundary to)
    {
        if (from <= to)
            throw new ArgumentOutOfRangeException(
                nameof(from),
                "From must be > than To");

        return (
            Segment1: new HashRange(from, HashRangeBoundary.Inclusive(config.MaxSlot)),
            Segment2: new HashRange(HashRangeBoundary.Inclusive(0), to)
        );
    }

    public int CompareTo(HashRange other)
    {
        if (this == other)
            return 0;

        return To > other.To
            ? 1
            : -1;
    }

    public bool Contains(ulong location)
    {
        return location >= From && location <= To;
    }

    public static bool operator <(HashRange range, ulong location)
    {
        return range.To.Value < location || !range.To.IsInclusive && range.To.Value == location;
    }

    public static bool operator >(HashRange range, ulong location)
    {
        return range.To.Value > location;
    }

    public double GetSize()
    {
        //both are inclusive in that case
        if (To == From)
            return 1;

        var delta = (double)To.Value - From.Value;
        if (To.IsInclusive && From.IsInclusive)
            return delta + 1;

        if (!To.IsInclusive && !From.IsInclusive)
            return delta - 1;

        return delta;
    }

    public override string ToString()
    {
        return $"{(From.IsInclusive ? '[' : '(')}{From.Value}, " +
               $"{To.Value}{(To.IsInclusive ? ']' : ')')}";
    }

    public string ToString(IRingConfiguration config)
    {
        return $"{ToString()} {config.GetShare(this):P2}";
    }

    public static HashRange Parse(string str)
    {
        var match = Regex.Match(str, @"(?<FBracket>[\(\[])(?<From>\d+),\s*(?<To>\d+)(?<TBracket>[\)\]])");

        if (!match.Success)
            throw new FormatException();

        return new HashRange(
            from: match.Groups["FBracket"].Value == "("
                ? HashRangeBoundary.Exclusive(ulong.Parse(match.Groups["From"].Value))
                : HashRangeBoundary.Inclusive(ulong.Parse(match.Groups["From"].Value)),
            to: match.Groups["TBracket"].Value == ")"
                ? HashRangeBoundary.Exclusive(ulong.Parse(match.Groups["To"].Value))
                : HashRangeBoundary.Inclusive(ulong.Parse(match.Groups["To"].Value))
        );
    }

    public HashRange? Intersect(HashRange other)
    {
        if (!(To >= other.From && From <= other.To))
            return null;

        var from = From.Value > other.From.Value || From.Value == other.From.Value && !From.IsInclusive
            ? From
            : other.From;

        var to = To.Value < other.To.Value || To.Value == other.To.Value && !To.IsInclusive
            ? To
            : other.To;

        if (from.Value == to.Value && (!from.IsInclusive || !to.IsInclusive))
            return null;

        return new HashRange(
            from: from,
            to: to);
    }

    public IEnumerable<ulong> Enumerate()
    {
        if (From.IsInclusive)
            yield return From.Value;

        var value = From.Value + 1;
        while (value < To.Value)
            yield return value++;

        if (To.IsInclusive && To.Value != From.Value)
            yield return To.Value;
    }
}