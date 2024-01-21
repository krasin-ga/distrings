using System.IO.Hashing;

namespace Distrings.Tests.Library;

public class XxHashAlgorithm : IHashAlgorithm
{
    public ulong CalculateHashCode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> destination = stackalloc byte[8];
        XxHash64.Hash(bytes, destination);

        return BitConverter.ToUInt64(destination);
    }
}