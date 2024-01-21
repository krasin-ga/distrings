namespace Distrings;

public interface ILookUpStrategyFactory<TNode>
    where TNode : IComparable<TNode>
{
    ILookUpStrategy<TNode> Create(
        IRingConfiguration ringConfiguration,
        IReadOnlyList<RingSegment<TNode>> ringSegments);
}