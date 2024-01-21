namespace Distrings;

public interface IPartitioningStrategy<TNode>
    where TNode : IComparable<TNode>
{
    IReadOnlyList<RingSegment<TNode>> CreatePartitions(IReadOnlyCollection<TNode> nodes);
}