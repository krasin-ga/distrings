namespace Distrings;

public class PingPong<TNode> : IPartitioningStrategy<TNode>
    where TNode : INode, IComparable<TNode>
{
    private readonly IRingConfiguration _ringConfiguration;

    public PingPong(IRingConfiguration ringConfiguration)
    {
        _ringConfiguration = ringConfiguration;
    }

    public IReadOnlyList<RingSegment<TNode>> CreatePartitions(
        IReadOnlyCollection<TNode> nodes)
    {
        var result = new RingSegment<TNode>[nodes.Count];
        var totalWeight = nodes.Sum(n => n.Weight);
        var segmentSize = (ulong)(_ringConfiguration.NumberOfSlots / (ulong)totalWeight);

        var from = 0UL;
        var sorted = PingPongSort(nodes);
        for (var i = 0; i < sorted.Count; i++)
        {
            var node = sorted[i];
            var to = from + segmentSize * node.Weight;

            if (i == sorted.Count - 1)
                to = _ringConfiguration.MaxSlot;

            result[i] = new RingSegment<TNode>(
                node,
                new HashRange(
                    from: i == 0
                        ? HashRangeBoundary.Inclusive(0UL)
                        : HashRangeBoundary.Exclusive(from),
                    to: HashRangeBoundary.Inclusive(to)
                ));

            from = to;
        }

        return result;
    }

    private static IReadOnlyList<TNode> PingPongSort(IReadOnlyCollection<TNode> nodes)
    {
        var oddIndex = 0;
        var evenIndex = nodes.Count - 1;
        var result = new TNode[nodes.Count];
        var index = 0;

        foreach (var node in nodes.OrderBy(n => n))
        {
            var resultIndex = index % 2 == 0
                ? oddIndex++
                : evenIndex--;

            result[resultIndex] = node;
            index++;
        }

        return result;
    }
}