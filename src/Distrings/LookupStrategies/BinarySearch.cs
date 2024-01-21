namespace Distrings;

public class BinarySearch<TNode> : ILookUpStrategy<TNode>
    where TNode : IComparable<TNode>
{
    private readonly ConnectedRingSegment<TNode>[] _sortedSegments;

    public ConnectedRingSegment<TNode> Head { get; }

    public BinarySearch(ConnectedRingSegments<TNode> connectedRingSegments)
    {
        _sortedSegments = connectedRingSegments.SortedSegments;
        Head = _sortedSegments[0];
    }

    public ConnectedRingSegment<TNode> LookUpSegment(ulong hashcode)
    {
        var left = 0;
        var right = _sortedSegments.Length - 1;

        while (left <= right)
        {
            var middle = (right + left) / 2;
            var middleSegment = _sortedSegments[middle];
            if (middleSegment.Range.Contains(hashcode))
                return middleSegment;

            if (middleSegment.Range < hashcode)
                left = middle + 1;
            else
                right = middle - 1;
        }

        return _sortedSegments[0];
    }

    public class Factory : ILookUpStrategyFactory<TNode>
    {
        public ILookUpStrategy<TNode> Create(
            IRingConfiguration _,
            IReadOnlyList<RingSegment<TNode>> ringSegments)
        {
            return new BinarySearch<TNode>(new ConnectedRingSegments<TNode>(ringSegments));
        }
    }
}