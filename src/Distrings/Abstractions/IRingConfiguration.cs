namespace Distrings;

public interface IRingConfiguration
{
    ulong MaxSlot { get; }
    double NumberOfSlots => (double)MaxSlot + 1;
    double GetShare(HashRange hashRange) => hashRange.GetSize() / NumberOfSlots;

    ulong ConstraintHashCode(ulong hashCode)
    {
        var maxSlot = MaxSlot;

        if (hashCode > maxSlot)
            return hashCode % (maxSlot + 1);

        return hashCode;
    }
}