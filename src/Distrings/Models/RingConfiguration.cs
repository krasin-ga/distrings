namespace Distrings;

public record RingConfiguration(ulong MaxSlot = ulong.MaxValue)
    : IRingConfiguration
{
    public static readonly IRingConfiguration Default = new RingConfiguration();

    public static IRingConfiguration OfConstrainedSize(uint size)
        => new RingConfiguration(MaxSlot: size - 1);
}