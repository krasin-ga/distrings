namespace Distrings.Tests.Library;

public static class Utilities
{
    public static void AssertWholeRingCoverage(
        IRingConfiguration ringConfiguration,
        IReadOnlyList<RingSegment<Node>> ringSegments)
    {
        Assert.Equal(
            expected: ringConfiguration.NumberOfSlots,
            actual: ringSegments.Sum(s => s.Range.GetSize()));
    }

    public static (Node Node, ulong HashCode)[] ParseNodesWithAngleHashes(
        IRingConfiguration ringConfiguration,
        string nodeAngles)
    {
        ulong ParseHashcode(string[] split)
        {
            var degrees = int.Parse(split[1][..^1]);
            if (degrees == 360)
                return ringConfiguration.MaxSlot;

            return (ulong)(ringConfiguration.MaxSlot * (degrees / 360d));
        }

        return nodeAngles
               .Split(" ", StringSplitOptions.RemoveEmptyEntries)
               .Select(n => n.Split(":", StringSplitOptions.RemoveEmptyEntries))
               .Select(n => (Node: new Node(n[0], Weight: 1), HashCode: ParseHashcode(n)))
               .ToArray();
    }

    public static RingSegment<Node>[] ParseSegments(
        string nodeRanges)
    {
        return nodeRanges.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                         .Select(n => n.Split(":", StringSplitOptions.RemoveEmptyEntries))
                         .Select(n => new RingSegment<Node>(Node: new Node(n[0], Weight: 1), Range: HashRange.Parse(n[1])))
                         .ToArray();
    }

    public static Node[] ParseNodesWithWeights(
        string nodeWeights)
    {
        return nodeWeights
               .Split(" ", StringSplitOptions.RemoveEmptyEntries)
               .Select(n => n.Split(":", StringSplitOptions.RemoveEmptyEntries))
               .Select(n => new Node(n[0], Weight: ushort.Parse(n[1])))
               .ToArray();
    }

    public static (Node[] Before, Node[] After) ParseRedistribution(
        string nodeWeightsWithRedistribution)
    {
        var split = nodeWeightsWithRedistribution.Split("->");
        return (ParseNodesWithWeights(split[0]), ParseNodesWithWeights(split[1]));
    }

    public static (Node Node, double Share)[] ParseExpectedShares(string nodesWithShares)
    {
        return nodesWithShares
               .Split(" ", StringSplitOptions.RemoveEmptyEntries)
               .Select(n => n.Split(":", StringSplitOptions.RemoveEmptyEntries))
               .Select(n => (Node: new Node(n[0], Weight: 1), HashCode: int.Parse(n[1][..^1]) / 100d))
               .ToArray();
    }

    public static ((Node Node, double Share)[] PerNode, double Total) ParseExpectedNonRemappedShares(string expectation)
    {
        var splitted = expectation.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var expectedNonRemappedSharesPerNode = ParseExpectedShares(splitted[0]);
        var expectedTotalNonRemappedShare = int.Parse(splitted[1].Trim()[..^1]) / 100d;

        return (expectedNonRemappedSharesPerNode, expectedTotalNonRemappedShare);
    }

    public static Dictionary<Node, (double Share, int SegmentsCount)> CalculateSummary(
        IRingConfiguration ringConfiguration,
        IEnumerable<RingSegment<Node>> ringSegments)
    {
        return ringSegments
               .GroupBy(g => g.Node)
               .ToDictionary(
                   r => r.Key,
                   grouping => (
                       Share: grouping.Sum(segment => ringConfiguration.GetShare(segment.Range)),
                       Count: grouping.Count()));
    }
}