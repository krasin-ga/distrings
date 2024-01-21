namespace Distrings;

public static class LookUpStrategyExtensions
{
    public static TNode LookUpNode<TNode>(this ILookUpStrategy<TNode> strategy, ulong hashcode)
        where TNode : IComparable<TNode> => strategy.LookUpSegment(hashcode).Node;

    public static IEnumerable<TNode> LookUpMany<TNode>(
        this ILookUpStrategy<TNode> strategy,
        ulong hashcode,
        int limit,
        IterationDirection direction = IterationDirection.Clockwise)
        where TNode : IComparable<TNode>
    {
        if (limit <= 0)
            yield break;

        var uniqueNodes = new HashSet<TNode>();
        foreach (var segment in strategy.LookUpSegment(hashcode).Iterate(direction))
        {
            var node = segment.Node;
            if (!uniqueNodes.Add(node))
                continue;
            yield return node;
            if (--limit <= 0)
                yield break;
        }
    }
}