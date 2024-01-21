using Distrings.Tests.Library;
using Xunit.Abstractions;

namespace Distrings.Tests;

public class PingPongTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PingPongTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
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
    public void MustProduceFairShareOfTheRingWithRespectToNodeWeight(
        string nodeWeights,
        string expectedShares)
    {
        var ringConfiguration = RingConfiguration.Default;
        var nodes = Utilities.ParseNodesWithWeights(nodeWeights);

        var pingPong = new PingPong<Node>(RingConfiguration.Default);

        var ringSegments = pingPong.CreatePartitions(nodes);
        var summary = Utilities.CalculateSummary(ringConfiguration, ringSegments);

        foreach (var (node, expectedShare) in Utilities.ParseExpectedShares(expectedShares))
        {
            var nodeSummary = summary[node];
            _testOutputHelper.WriteLine($"{node.Identity}: {nodeSummary.SegmentsCount} -> {nodeSummary.Share}");

            Assert.Equal(
                expected: expectedShare,
                actual: Math.Round(nodeSummary.Share, 2));
        }
    }

    [Theory]
    [InlineData(
        "A:1 -> A:1 B:1",
        "A:50% | 50%")]
    [InlineData(
        "A:1 B:1 -> A:1 B:1 C:1",
        "A:67% B:67% | 67%")]
    [InlineData(
        "A:1 B:1 C:1 -> A:1 B:1 C:1 D:1",
        "A:75% B:75% C:50% | 67%")]
    [InlineData(
        "A:1 B:1 C:1 D:1 -> A:1 B:1 C:1 D:1 E:1",
        "A:80% B:80% C:60% D:60% | 70%")]
    [InlineData(
        "A:1 B:1 -> A:1 B:1 C:2",
        "A:50% B:50% | 50%")]
    public void MustCorrectlyRemapRanges(
        string nodesDistribution,
        string expectedNonRemappedShares)
    {
        var ringConfiguration = RingConfiguration.Default;
        var (initialNodes, redistributedNodes) = Utilities.ParseRedistribution(nodesDistribution);

        var consistentHashing = new PingPong<Node>(ringConfiguration);

        var initialPartitions = consistentHashing.CreatePartitions(initialNodes);
        var redistributedPartitions = consistentHashing.CreatePartitions(redistributedNodes);

        var (expectedNonRemappedSharesPerNode, expectedTotalNonRemappedShare)
            = Utilities.ParseExpectedNonRemappedShares(expectedNonRemappedShares);

        var totalNonRemapped = 0d;
        foreach (var (node, expectedShare) in expectedNonRemappedSharesPerNode)
        {
            var initial = initialPartitions.Where(s => s.Node.Equals(node)).ToArray();
            var redistributed = redistributedPartitions.Where(s => s.Node.Equals(node)).ToArray();

            var intersectedSize = redistributed
                                  .SelectMany(r => initial.Select(i => i.Range.Intersect(r.Range)))
                                  .Sum(r => r?.GetSize() ?? 0);

            totalNonRemapped += intersectedSize;

            var initialSize = initial.Sum(r => r.Range.GetSize());

            var nonRemappedShare = intersectedSize / initialSize;
            _testOutputHelper.WriteLine($"{node.Identity}: {nonRemappedShare:F2}");

            Assert.Equal(
                expected: expectedShare,
                actual: Math.Round(nonRemappedShare, 2));
        }

        Assert.Equal(expected: expectedTotalNonRemappedShare,
                     actual: Math.Round(totalNonRemapped / ringConfiguration.NumberOfSlots, 2));
    }
}