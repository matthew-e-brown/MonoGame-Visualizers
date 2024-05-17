namespace SortVisualizer;

/// <summary>
/// A wrapper for an array that keeps track of all actions performed on its elements.
/// </summary>
public class ArrayProxy
{
    protected SortableItem[] Values { get; init; }
    protected Visualizer Parent { get; }

    // ----------------------------------------

    internal ArrayProxy(Visualizer parent, IEnumerable<int> startingValues)
    {
        Parent = parent;
        Values = startingValues
            .Select((value, index) => new SortableItem(parent, value, index))
            .ToArray();
    }

    // ----------------------------------------

    public SortableItem this[int index]
    {
        get => Values[index];
        set
        {
            // [TODO] When an index is assigned to, generate a `Move` action based on where the incoming element came
            // from.
            throw new NotImplementedException();
        }
    }

    // [TODO] Implement Swap, Insert, etc. with their own action types
}
