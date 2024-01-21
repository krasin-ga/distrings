namespace Distrings;

public interface ILookUpStrategy<TNode>
    where TNode : IComparable<TNode>
{
    public ConnectedRingSegment<TNode> Head { get; }
    ConnectedRingSegment<TNode> LookUpSegment(ulong hashcode);
}