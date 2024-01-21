using System.Text;

namespace Distrings;

public record ConnectedRingSegment<TNode>(RingSegment<TNode> RingSegment)
    where TNode : IComparable<TNode>
{
    public ConnectedRingSegment<TNode>? Next { get; private set; }
    public ConnectedRingSegment<TNode>? Previous { get; private set; }

    public TNode Node => RingSegment.Node;
    public HashRange Range => RingSegment.Range;

    public void ConnectWithNext(ConnectedRingSegment<TNode> next)
    {
        Next = next;
        next.Previous = this;
    }

    public void ConnectWithPrevious(ConnectedRingSegment<TNode> previous)
    {
        previous.Next = this;
        Previous = previous;
    }

    public IEnumerable<ConnectedRingSegment<TNode>> Iterate(IterationDirection direction = IterationDirection.Clockwise)
    {
        var currentSegment = this;

        while (currentSegment != null)
        {
            yield return currentSegment;

            currentSegment = direction switch
            {
                IterationDirection.Clockwise => currentSegment.Next,
                IterationDirection.Counterclockwise => currentSegment.Previous,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            if (ReferenceEquals(currentSegment, this))
                yield break;
        }
    }

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append(Previous?.Node.ToString()).Append(" <-")
               .Append(Node).Append(' ').Append(Range)
               .Append("->").Append(Next?.Node.ToString());

        return true;
    }
}