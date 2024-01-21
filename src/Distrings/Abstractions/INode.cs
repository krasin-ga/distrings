namespace Distrings;

public interface INode
{
    /// <summary>
    /// Stable identity of the node
    /// </summary>
    string Identity { get; }

    /// <summary>
    /// Relative weight of the node
    /// </summary>
    ushort Weight { get; }
}