namespace Distrings;

public readonly record struct NodeHash<TNode>(TNode Node, ulong Hash)
    where TNode : IComparable<TNode>
{
    public static IComparer<NodeHash<TNode>> Comparer { get; } = new HashNodeRelationalComparer();

    private sealed class HashNodeRelationalComparer : IComparer<NodeHash<TNode>>
    {
        public int Compare(NodeHash<TNode> x, NodeHash<TNode> y)
        {
            var hashComparison = x.Hash.CompareTo(y.Hash);

            return hashComparison != 0
                ? hashComparison
                : x.Node.CompareTo(y.Node);
        }
    }
}