namespace Distrings;

public readonly record struct RingSegment<TNode>(TNode Node, HashRange Range)
    where TNode : IComparable<TNode>;