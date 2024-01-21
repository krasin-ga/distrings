namespace Distrings;

public interface IHashAlgorithm
{
    ulong CalculateHashCode(ReadOnlySpan<byte> bytes);
}