namespace Distrings.Tests.Library;

public class FakeFairHashAlgorithm : IHashAlgorithm
{
    private readonly ulong _increment;
    private ulong _callNumber;

    public FakeFairHashAlgorithm(
        IRingConfiguration ringConfiguration,
        ulong numberOfCalls)
    {
        _increment = ringConfiguration.MaxSlot / numberOfCalls;
    }

    public ulong CalculateHashCode(ReadOnlySpan<byte> bytes)
    {
        return _increment * ++_callNumber;
    }
}