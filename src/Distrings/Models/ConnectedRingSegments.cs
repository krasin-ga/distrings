namespace Distrings;

public class ConnectedRingSegments<TNode>
    where TNode : IComparable<TNode>
{
    public ConnectedRingSegment<TNode>[] SortedSegments { get; }

    public ConnectedRingSegments(IReadOnlyCollection<RingSegment<TNode>> value)
        : this(CreateConnectedSegments(value))
    {
    }

    public ConnectedRingSegments(ConnectedRingSegment<TNode>[] sortedSegments)
    {
        SortedSegments = sortedSegments;
    }

    private static ConnectedRingSegment<TNode>[] CreateConnectedSegments(
        IReadOnlyCollection<RingSegment<TNode>> value)
    {
        var sorted = value.OrderBy(ringSegment => ringSegment.Range)
                          .ThenBy(ringSegment => ringSegment.Node);

        var result = new ConnectedRingSegment<TNode>[value.Count];

        ConnectedRingSegment<TNode>? first = null;
        ConnectedRingSegment<TNode>? previous = null;
        var index = 0;

        foreach (var ringSegment in sorted)
        {
            var current = new ConnectedRingSegment<TNode>(ringSegment);
            if (previous is { })
                current.ConnectWithPrevious(previous);
            else
                first = current;

            previous = current;

            result[index++] = current;
        }

        if (previous is { } && first is { })
            previous.ConnectWithNext(first);

        return result;
    }
}