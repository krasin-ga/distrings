using Xunit.Abstractions;

namespace Distrings.Tests;

public class HashRangeTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HashRangeTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("(0, 1) 0,00 %")]
    [InlineData("[0, 0] 0,00 %")]
    [InlineData("[0, 1) 0,00 %")]
    [InlineData("(0, 1] 0,00 %")]
    [InlineData("[0, 1] 0,00 %")]
    [InlineData("[0, 18446744073709551615] 100,00 %")]
    [InlineData("[0, 6148914691236517205] 33,33 %")]
    public void MustCorrectlyFormatAndParse(string formatted)
    {
        var ringConfiguration = RingConfiguration.Default;

        var parsed = HashRange.Parse(formatted);
        Assert.Equal(expected: formatted, actual: parsed.ToString(ringConfiguration));
    }

    [Theory]
    [InlineData("(0, 1)", 0)]
    [InlineData("[0, 0]", 1)]
    [InlineData("[0, 1)", 1)]
    [InlineData("(0, 1]", 1)]
    [InlineData("[0, 1]", 2)]
    [InlineData("(0, 2)", 1)]
    [InlineData("(0, 2]", 2)]
    [InlineData("[0, 2)", 2)]
    [InlineData("[0, 2]", 3)]
    [InlineData("[0, 18446744073709551615]", 18446744073709551616d)]
    public void MustCorrectlyCalculateSize(string range, double expectedSize)
    {
        Assert.Equal(expected: expectedSize, actual: HashRange.Parse(range).GetSize());
    }

    [Theory]
    [InlineData("[0, 1]", "[0, 1]", "[0, 1]")]
    [InlineData("[0, 1]", "[0, 1)", "[0, 1)")]
    [InlineData("[0, 1]", "(0, 1]", "(0, 1]")]
    [InlineData("(0, 1)", "(0, 1)", "(0, 1)")]
    [InlineData("(0, 1)", "(0, 2)", "(0, 1)")]
    [InlineData("(0, 1)", "(2, 3)", null)]
    [InlineData("(0, 1)", "(1, 2)", null)]
    [InlineData("(0, 1)", "[1, 2)", null)]
    [InlineData("(0, 5)", "[1, 3]", "[1, 3]")]
    [InlineData("(0, 5)", "[1, 3)", "[1, 3)")]
    [InlineData("(0, 5)", "(1, 3]", "(1, 3]")]
    [InlineData("(0, 5)", "(1, 3)", "(1, 3)")]
    [InlineData("(0, 5)", "(2, 10)", "(2, 5)")]
    [InlineData("[3, 9]", "[2, 5]", "[3, 5]")]
    [InlineData("(3, 9)", "(2, 5)", "(3, 5)")]
    public void MustCorrectlyIntersect(
        string rangeA,
        string rangeB,
        string? expectedRange)
    {
        var a = HashRange.Parse(rangeA);
        var b = HashRange.Parse(rangeB);
        HashRange? expected = expectedRange is { }
            ? HashRange.Parse(expectedRange)
            : null;

        var ab = a.Intersect(b);
        var ba = b.Intersect(a);

        _testOutputHelper.WriteLine($"ab = {ab}");
        _testOutputHelper.WriteLine($"ba = {ba}");

        Assert.Equal(expected, ab);
        Assert.Equal(expected, ba);
    }
}