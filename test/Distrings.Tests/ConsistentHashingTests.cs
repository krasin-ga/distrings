using System.Text;
using Distrings.Tests.Library;
using Xunit.Abstractions;

namespace Distrings.Tests;

public class ConsistentHashingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ConsistentHashingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(
        "A:180° B:360°",
        "A:50%  B:50%")]
    [InlineData(
        "A:120° B:240° C:360°",
        "A:33%  B:33%  C:33%")]
    [InlineData(
        "A:90° B:180° C:270° D:360°",
        "A:25% B:25%  C:25%  D:25%")]
    [InlineData(
        "A:90° B:180°",
        "A:75% B:25%")]
    [InlineData(
        "A:90°",
        "A:100%")]
    [InlineData(
        "A:0° B:0°",
        "A:100%")]
    public void MustCorrectlyPlaceNodesOnRing(
        string nodeAngles,
        string expectedShares)
    {
        var ringConfiguration = RingConfiguration.Default;
        var nodesWithHashes = Utilities.ParseNodesWithAngleHashes(ringConfiguration, nodeAngles);

        var consistentHashing = new ConsistentHashing<Node>(
            ringConfiguration,
            new PredefinedHashAlgorithm(
                nodesWithHashes
                    .Select(n => (n.Node.Identity, n.HashCode))
                    .ToArray()
            )
        );

        var nodes = nodesWithHashes.Select(n => n.Node).ToArray();
        var ringSegments = consistentHashing.CreatePartitions(nodes);
        var shares = Utilities.CalculateSummary(ringConfiguration, ringSegments);

        foreach (var (node, expectedShare) in Utilities.ParseExpectedShares(expectedShares))
            Assert.Equal(
                expected: expectedShare,
                actual: shares[node].Share,
                tolerance: 0.01);

        Utilities.AssertWholeRingCoverage(ringConfiguration, ringSegments);
    }

    [Theory]
    [InlineData(
        "A:3  B:3  C:3",
        "A:33% B:33% C:33%")]
    [InlineData(
        "A:10  B:10  C:10",
        "A:33% B:33% C:33%")]
    [InlineData(
        "A:100  B:100  C:100",
        "A:33%  B:33%  C:33%")]
    [InlineData(
        "A:256  B:256  C:256",
        "A:33%  B:33%  C:33%")]
    [InlineData(
        "A:2   B:10",
        "A:17%  B:83%")]
    [InlineData(
        "A:20   B:100",
        "A:17%  B:83%")]
    public void MustRespectNodeWeight(
        string nodeWeights,
        string expectedShares)
    {
        var ringConfiguration = RingConfiguration.Default;
        var nodes = Utilities.ParseNodesWithWeights(nodeWeights);

        var consistentHashing = new ConsistentHashing<Node>(
            ringConfiguration,
            new FakeFairHashAlgorithm(
                ringConfiguration, 
                numberOfCalls: (ulong)nodes.Sum(n => n.Weight))
        );

        var ringSegments = consistentHashing.CreatePartitions(nodes);
        var summary = Utilities.CalculateSummary(ringConfiguration, ringSegments);

        foreach (var (node, expectedShare) in Utilities.ParseExpectedShares(expectedShares))
        {
            var nodeSummary = summary[node];

            _testOutputHelper.WriteLine($"{node.Identity}: {nodeSummary.SegmentsCount} -> {nodeSummary.Share}");
            var nodeWeight = nodes.Single(n => n == node).Weight;

            Assert.Equal(
                (double)nodeWeight,
                nodeSummary.SegmentsCount,
                tolerance: 1); //+1 is expected for "loop around segment"

            Assert.Equal(
                expected: expectedShare,
                actual: Math.Round(nodeSummary.Share, 2));
        }

        Utilities.AssertWholeRingCoverage(ringConfiguration, ringSegments);
    }

    [Theory]
    [InlineData(
        "A:10  B:10  C:10",
        "A:33% B:33% C:33%",
        0.06d)]
    [InlineData(
        "A:100  B:100  C:100",
        "A:33%  B:33%  C:33%",
        0.051d)]
    [InlineData(
        "A:256  B:256  C:256",
        "A:33%  B:33%  C:33%",
        0.04d)]
    [InlineData(
        "A:1000  B:1000  C:1000",
        "A:33%   B:33%    C:33%",
        0.027d)]
    [InlineData(
        "A:2   B:10",
        "A:17%  B:83%",
        0.085d)]
    [InlineData(
        "A:20   B:100",
        "A:17%  B:83%",
        0.065d)]
    [InlineData(
        "A:40   B:200",
        "A:17%  B:83%",
        0.05d)]
    public void MustRespectNodeWeightWithRealHashingAlgorithm(
        string nodeWeights,
        string expectedShares,
        double shareTolerance)
    {
        var ringConfiguration = RingConfiguration.Default;

        var nodes = Utilities.ParseNodesWithWeights(nodeWeights);

        var consistentHashing = new ConsistentHashing<Node>(
            ringConfiguration,
            new XxHashAlgorithm()
        );

        var ringSegments = consistentHashing.CreatePartitions(nodes);
        var summary = Utilities.CalculateSummary(ringConfiguration, ringSegments);

        foreach (var (node, expectedShare) in Utilities.ParseExpectedShares(expectedShares))
        {
            var nodeSummary = summary[node];
            _testOutputHelper.WriteLine($"{node.Identity}: {nodeSummary.SegmentsCount} -> {nodeSummary.Share}");
            var nodeWeight = nodes.Single(n => n == node).Weight;

            Assert.Equal(
                (double)nodeWeight,
                nodeSummary.SegmentsCount,
                tolerance: 1); //+1 is expected for "loop around segment"

            Assert.Equal(
                expected: expectedShare,
                actual: nodeSummary.Share,
                shareTolerance); //account for distribution imperfection
        }

        Utilities.AssertWholeRingCoverage(ringConfiguration, ringSegments);
    }

    private class PredefinedHashAlgorithm : IHashAlgorithm
    {
        private readonly Dictionary<string, ulong> _predefinedHashes;

        public PredefinedHashAlgorithm(
            params (string Key, ulong Value)[] predefinedHashes)
        {
            _predefinedHashes = predefinedHashes.ToDictionary(
                k => k.Key,
                v => v.Value
            );
        }

        public ulong CalculateHashCode(ReadOnlySpan<byte> bytes)
        {
            var targetBytes = bytes.ToArray();

            foreach (var (key, value) in _predefinedHashes)
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);

                if (targetBytes.Take(keyBytes.Length).SequenceEqual(keyBytes))
                    return value;
            }

            throw new InvalidOperationException();
        }
    }
}