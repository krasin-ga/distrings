using Distrings.Tests.Library;

namespace Distrings.Tests;

public class BinarySearchTests
{
    [Theory]
    [InlineData("A:[0, 1] - B:(1, 2]")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 40] - E:[0, 0]")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 40] - E:[0, 0] - D:[100, 2000)")]
    [InlineData("A:[1, 10] - A:[11, 20] - C:[21, 30] - D:(30, 40] - E:[0, 0] - D:[100, 2000)")]
    [InlineData("A:[0, 50000] - B:(50000, 60000] - A:(18446744073709551610, 18446744073709551615]")]
    public void MustLocateCorrectNode(
        string nodesWithRanges)
    {
        var ringSegments = Utilities.ParseSegments(nodesWithRanges);
        var lookUpStrategy = new BinarySearch<Node>(
            new ConnectedRingSegments<Node>(ringSegments)
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
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 22, 4, "C, D, E, A")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 1, 1, "A")]
    [InlineData("A:[1, 10] - B:[11, 20] - C:[21, 30] - D:(30, 39] - E:[0, 0]", 1, 2, "A, B")]
    public void MustCorrectlyLocateMultipleNodes(
        string nodesWithRanges,
        ulong hashCodeToLookup,
        int limit,
        string expectedNodes)
    {
        var ringSegments = Utilities.ParseSegments(nodesWithRanges);
        var lookUpStrategy = new BinarySearch<Node>(
            new ConnectedRingSegments<Node>(ringSegments)
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