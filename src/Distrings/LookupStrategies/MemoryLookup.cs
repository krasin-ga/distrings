namespace Distrings;

public class MemoryLookup<TNode> : ILookUpStrategy<TNode>
    where TNode : IComparable<TNode>
{
    private readonly ConnectedRingSegment<TNode>[] _cache;
    private readonly ulong _numberOfSlots;
    public ConnectedRingSegment<TNode> Head { get; }

    public MemoryLookup(
        ConnectedRingSegments<TNode> connectedRingSegments,
        IRingConfiguration ringConfiguration)
    {
        var numberOfSlots = ringConfiguration.NumberOfSlots;
        if (numberOfSlots > int.MaxValue)
            throw new Exception("Ring is too big to fit in memory. Consider reducing number of slots");

        _numberOfSlots = (ulong)numberOfSlots;
        _cache = new ConnectedRingSegment<TNode>[(int)_numberOfSlots];

        foreach (var segment in connectedRingSegments.SortedSegments)
        {
            var range = segment.Range;

            var index = range.From.IsInclusive
                ? range.From.Value
                : range.From.Value + 1;

            Array.Fill(_cache, segment, (int)index, (int)range.GetSize());
        }

        Head = connectedRingSegments.SortedSegments[0];
    }

    public ConnectedRingSegment<TNode> LookUpSegment(ulong hashcode)
    {
        if (hashcode > _numberOfSlots)
            throw new ArgumentOutOfRangeException(
                nameof(hashcode),
                "The hash code exceeds the number of slots. " +
                "Consider applying the modulo operator to hash codes.");

        return _cache[(int)hashcode];
    }

    public class Factory : ILookUpStrategyFactory<TNode>
    {
        public ILookUpStrategy<TNode> Create(
            IRingConfiguration ringConfiguration,
            IReadOnlyList<RingSegment<TNode>> ringSegments)
        {
            return new MemoryLookup<TNode>(
                new ConnectedRingSegments<TNode>(ringSegments),
                ringConfiguration);
        }
    }
}