using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Distrings;
internal static class NodeHashPool<TNode>
    where TNode : IComparable<TNode>
{
    public static readonly ArrayPool<NodeHash<TNode>> Instance
        = ArrayPool<NodeHash<TNode>>.Create();
}

public class ConsistentHashing<TNode> : IPartitioningStrategy<TNode>
    where TNode : INode, IComparable<TNode>
{
    private readonly IHashAlgorithm _hashAlgorithm;
    private readonly IRingConfiguration _ringConfiguration;

    public ConsistentHashing(
        IRingConfiguration ringConfiguration,
        IHashAlgorithm hashAlgorithm)
    {
        _ringConfiguration = ringConfiguration;
        _hashAlgorithm = hashAlgorithm;
    }

    public IReadOnlyList<RingSegment<TNode>> CreatePartitions(
        IReadOnlyCollection<TNode> nodes)
    {
        if (nodes.Count == 0)
            return Array.Empty<RingSegment<TNode>>();

        if (nodes.Count == 1)
            return new RingSegment<TNode>[]
                   {
                       new(nodes.First(), HashRange.FullCoverage(_ringConfiguration))
                   };

        var totalCount = nodes.Sum(n => n.Weight);
        var rentedNodeHashes = NodeHashPool<TNode>.Instance.Rent(
            minimumLength: totalCount);

        var offset = 0;
        foreach (var node in nodes)
        {
            if (node.Weight == 0)
                continue;

            WriteHashes(node, rentedNodeHashes.AsSpan(offset, node.Weight));

            offset += node.Weight;
        }

        var hashes = rentedNodeHashes.AsSpan(0, totalCount);
        hashes.Sort(NodeHash<TNode>.Comparer);
        var previousHash = hashes[^1].Hash;

        var result = new List<RingSegment<TNode>>(totalCount + 1);

        for (var i = 0; i < hashes.Length; i++)
        {
            var nodeHash = hashes[i];
            if (nodeHash.Hash == previousHash && i > 0)
                continue;

            var from = HashRangeBoundary.Exclusive(previousHash);
            var to = HashRangeBoundary.Inclusive(nodeHash.Hash);

            if (from <= to)
            {
                result.Add(
                    new RingSegment<TNode>(
                        nodeHash.Node,
                        new HashRange(
                            from: from,
                            to: to
                        )
                    ));

                previousHash = nodeHash.Hash;
                continue;
            }

            if (from.Value == _ringConfiguration.MaxSlot)
            {
                result.Add(
                    new RingSegment<TNode>(
                        nodeHash.Node,
                        new HashRange(
                            HashRangeBoundary.Inclusive(0),
                            to)
                    )
                );

                previousHash = nodeHash.Hash;
                continue;
            }

            var (segment1, segment2) = HashRange.LoopAround(_ringConfiguration, from, to);

            result.Add(
                new RingSegment<TNode>(
                    nodeHash.Node,
                    segment1
                ));

            result.Add(
                new RingSegment<TNode>(
                    nodeHash.Node,
                    segment2
                ));

            previousHash = nodeHash.Hash;
        }

        NodeHashPool<TNode>.Instance.Return(rentedNodeHashes);

        return result;
    }

    private void WriteHashes(TNode node, Span<NodeHash<TNode>> destination)
    {
        const int maxSizeOnStack = 256;
        var maxByteCount =
            Encoding.UTF8.GetMaxByteCount(node.Identity.Length)
            + sizeof(int);

        byte[]? rentedBytes = null;
        try
        {
            var bytes = maxByteCount > maxSizeOnStack
                ? rentedBytes = ArrayPool<byte>.Shared.Rent(maxByteCount)
                : stackalloc byte[maxSizeOnStack];

            var bytesWritten = Encoding.UTF8.GetBytes(node.Identity, bytes);

            var target = bytes[..(bytesWritten + sizeof(int))];
            var vNodeBytes = target[^sizeof(int)..];
            for (var i = 0; i < destination.Length; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(vNodeBytes, i);
                destination[i] = new NodeHash<TNode>(
                    node,
                    _ringConfiguration.ConstraintHashCode(
                        _hashAlgorithm.CalculateHashCode(target)));
            }
        }
        finally
        {
            if (rentedBytes is { })
                ArrayPool<byte>.Shared.Return(rentedBytes);
        }
    }
}