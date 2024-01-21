using Distrings.Tests.Library;

namespace Distrings.Tests;

public class MemoryLookupTests
{
    [Theory]
    [InlineData("A:[0, 1] - B:(1, 2]", 3)]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 40)]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 40] - E:[0, 0] - D:[100, 2000)", 2000)]
    [InlineData("A:[1, 10] - A:[11, 20] - C:[21, 30] - D:(30, 40] - E:[0, 0] - D:[100, 2000)", 2000)]
    [InlineData("A:[0, 50000] - B:(50000, 60000)", 60000)]
    [InlineData("A:[0, 50000] - B:(50000, 60000]", 60001)]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[127999, 128010]", 128011)]

    public void MustLocateCorrectNode(
        string nodesWithRanges,
        uint ringSize)
    {
        var ringSegments = Utilities.ParseSegments(nodesWithRanges);
        var lookUpStrategy = new MemoryLookup<Node>(
            new ConnectedRingSegments<Node>(ringSegments),
            RingConfiguration.OfConstrainedSize(ringSize)
        );

        foreach (var segment in ringSegments)
        {
            foreach (var value in segment.Range.Enumerate())
            {
                var located = lookUpStrategy.LookUpNode(value);
                Assert.Equal(expected: segment.Node.Identity, actual: located.Identity);
            }
        }
    }

    [Theory]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 40, 22, 4, "C, D, E, A")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 40, 1, 1, "A")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 40, 1, 2, "A, B")]
    public void MustCorrectlyLocateMultipleNodes(
        string nodesWithRanges, 
        uint ringSize,
        ulong hashCodeToLookup,
        int limit,
        string expectedNodes)
    {
        var ringSegments = Utilities.ParseSegments(nodesWithRanges);
        var lookUpStrategy = new MemoryLookup<Node>(
            new ConnectedRingSegments<Node>(ringSegments),
            RingConfiguration.OfConstrainedSize(ringSize)
        );

        var nodes = lookUpStrategy.LookUpMany(hashCodeToLookup, limit).ToArray();

        var expected = expectedNodes.Split(',', StringSplitOptions.TrimEntries);

        Assert.Equal(
            expected: expected.Length, 
            actual: nodes.Length);

        foreach (var (left, right) in nodes.Zip(expected))
            Assert.Equal(left.Identity, right);
    }
}