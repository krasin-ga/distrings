namespace Distrings;

public record Node(string Identity, ushort Weight)
    : INode, IComparable<Node>
{
    public int CompareTo(Node? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;
        return string.Compare(Identity, other.Identity, StringComparison.Ordinal);
    }

    public virtual bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Identity == other.Identity;
    }

    public override int GetHashCode()
    {
        return Identity.GetHashCode();
    }
}

