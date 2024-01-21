using System.Text;

namespace Distrings;

public class HashRing<TNode>
    where TNode : IComparable<TNode>
{
    private readonly ILookUpStrategy<TNode> _lookupStrategy;
    private readonly ILookUpStrategyFactory<TNode> _lookUpStrategyFactory;
    private readonly IPartitioningStrategy<TNode> _partitioningStrategy;
    private readonly IRingConfiguration _ringConfiguration;
    private IReadOnlyDictionary<TNode, NodeSummary>? _summary;
    public ConnectedRingSegment<TNode> Head => _lookupStrategy.Head;

    public HashRing(
        IRingConfiguration ringConfiguration,
        IPartitioningStrategy<TNode> partitioningStrategy,
        ILookUpStrategyFactory<TNode> lookUpStrategyFactory,
        IReadOnlyList<TNode> nodes)
    {
        if (nodes.Count == 0)
            throw new ArgumentException("Value cannot be an empty collection.", nameof(nodes));

        _lookupStrategy = lookUpStrategyFactory.Create(
            ringConfiguration,
            partitioningStrategy.CreatePartitions(nodes));
        _ringConfiguration = ringConfiguration;
        _lookUpStrategyFactory = lookUpStrategyFactory;
        _partitioningStrategy = partitioningStrategy;
    }

    public HashRing<TNode> Rebuild(IReadOnlyList<TNode> nodes)
    {
        return new HashRing<TNode>(
            _ringConfiguration,
            _partitioningStrategy,
            _lookUpStrategyFactory,
            nodes);
    }

    public TNode GetNode(ulong hashCode)
        => _lookupStrategy.LookUpNode(_ringConfiguration.ConstraintHashCode(hashCode));

    public IEnumerable<TNode> GetNodes(
        ulong hashCode,
        int limit,
        IterationDirection direction = IterationDirection.Clockwise)
        => _lookupStrategy.LookUpMany(
            _ringConfiguration.ConstraintHashCode(hashCode),
            limit,
            direction);

    public TNode GetNode<TKey>(TKey key, Func<TKey, ulong> getHashCode)
        => GetNode(getHashCode(key));

    public ConnectedRingSegment<TNode> GetSegment(ulong hashCode)
        => _lookupStrategy.LookUpSegment(_ringConfiguration.ConstraintHashCode(hashCode));

    public ConnectedRingSegment<TNode> GetSegment<TKey>(
        TKey key,
        Func<TKey, ulong> getHashCode)
        => GetSegment(getHashCode(key));

    public IReadOnlyDictionary<TNode, NodeSummary> CalculateSummary()
    {
        if (_summary is { })
            return _summary;

        _summary = Head
                   .Iterate()
                   .GroupBy(g => g.Node)
                   .ToDictionary(
                       r => r.Key,
                       grouping => new NodeSummary(
                           TotalShare: grouping.Sum(segment => _ringConfiguration.GetShare(segment.Range)),
                           NumberOfSegments: grouping.Count()));

        return _summary;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var (node, summary) in CalculateSummary().OrderBy(k => k.Key))
        {
            stringBuilder
                .Append(node).Append(' ')
                .Append("Share=").Append(summary.TotalShare.ToString("P2")).Append(' ')
                .Append("Segments=").Append(summary.NumberOfSegments).AppendLine();
        }

        return stringBuilder.ToString();
    }

    public record NodeSummary(double TotalShare, int NumberOfSegments);
}