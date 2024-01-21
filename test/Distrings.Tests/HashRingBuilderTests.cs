using System.Text;
using Distrings.Tests.Library;
using Xunit.Abstractions;

namespace Distrings.Tests;

public class HashRingBuilderTests
{
    private const int ConstrainedSize = 128_000;

    private static readonly Node[] Nodes
        = new[]
          {
              new Node(Identity: "node_01", Weight: 10),
              new Node(Identity: "node_02", Weight: 10),
              new Node(Identity: "node_03", Weight: 30),
              new Node(Identity: "node_04", Weight: 10),
          };

    private readonly ITestOutputHelper _testOutputHelper;

    public HashRingBuilderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CreateRing_ConsistentHashingWithBinarySearch_CreatesValidRing()
    {
        var hashAlgorithm = new XxHashAlgorithm();

        var ring = new HashRingBuilder<Node>(RingConfiguration.Default)
                   .PartitionWith(t => t.ConsistentHashing(hashAlgorithm))
                   .LookupWith(l => l.BinarySearch())
                   .CreateRing(Nodes);

        PerformAssertions(ring, hashAlgorithm);
    }

    [Fact]
    public void CreateRing_ConsistentHashingWithMemoryLookup_CreatesValidRing()
    {
        var hashAlgorithm = new XxHashAlgorithm();

        var ring = new HashRingBuilder<Node>(RingConfiguration.OfConstrainedSize(ConstrainedSize))
                   .PartitionWith(t => t.ConsistentHashing(hashAlgorithm))
                   .LookupWith(l => l.MemoryLookup())
                   .CreateRing(Nodes);

        PerformAssertions(ring, hashAlgorithm);
    }

    [Fact]
    public void CreateRing_PingPongWithBinarySearch_CreatesValidRing()
    {
        var hashAlgorithm = new XxHashAlgorithm();

        var ring = new HashRingBuilder<Node>(RingConfiguration.Default)
                   .PartitionWith(t => t.PingPong())
                   .LookupWith(l => l.BinarySearch())
                   .CreateRing(Nodes);

        PerformAssertions(ring, hashAlgorithm);
    }

    [Fact]
    public void CreateRing_PingPongWithMemoryLookup_CreatesValidRing()
    {
        var hashAlgorithm = new XxHashAlgorithm();

        var ringConfiguration = RingConfiguration.OfConstrainedSize(ConstrainedSize);

        var ring = new HashRingBuilder<Node>(ringConfiguration)
                   .PartitionWith(t => t.PingPong())
                   .LookupWith(l => l.MemoryLookup())
                   .CreateRing(Nodes);

        Utilities.AssertWholeRingCoverage(
            ringConfiguration,
            ring.Head.Iterate().Select(s => s.RingSegment).ToArray());

        PerformAssertions(ring, hashAlgorithm);
    }

    private void PerformAssertions(HashRing<Node> ring, XxHashAlgorithm hashAlgorithm)
    {
        var formattedRing = ring.ToString();
        Assert.False(string.IsNullOrWhiteSpace(formattedRing));
        _testOutputHelper.WriteLine(formattedRing);

        for (var i = 0; i < 512_000; i++)
        {
            var userNode = GetNodeByString(
                stringId: Guid.NewGuid().ToString(),
                hashAlgorithm,
                ring);

            Assert.NotNull(userNode);
        }

        var random = new Random();
        for (var i = 0; i < 512_000; i++)
        {
            var idNode = GetNodeByULong(
                id: (ulong)(random.NextDouble() * ulong.MaxValue),
                hashAlgorithm,
                ring);

            Assert.NotNull(idNode);
        }
    }

    private static Node GetNodeByString(
        string stringId,
        IHashAlgorithm hashAlgorithm,
        HashRing<Node> ring)
    {
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(stringId.Length);

        const int maxBytesOnStack = 128;
        var userIdBytes = maxByteCount <= maxBytesOnStack
            ? stackalloc byte[maxBytesOnStack]
            : new byte[maxByteCount];

        userIdBytes = userIdBytes[..Encoding.UTF8.GetBytes(stringId, userIdBytes)];
        var userIdHash = hashAlgorithm.CalculateHashCode(userIdBytes);

        return ring.GetNode(hashCode: userIdHash);
    }

    private static Node GetNodeByULong(
        ulong id,
        IHashAlgorithm hashAlgorithm,
        HashRing<Node> ring)
    {
        Span<byte> itemIdBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(itemIdBytes, id);
        var userIdHash = hashAlgorithm.CalculateHashCode(itemIdBytes);

        return ring.GetNode(hashCode: userIdHash);
    }
}