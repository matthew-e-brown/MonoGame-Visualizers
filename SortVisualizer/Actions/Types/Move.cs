namespace SortVisualizer.Actions;

/// <summary>
/// An item has been moved from one location in the visualizer to another.
/// </summary>
public record Move : Action
{
    public readonly Location From;
    public readonly Location To;
    public readonly int Value;
    public readonly int? Overwrote; // value, not index

    public Move(int value, Location from, Location to)
    {
        Value = value;
        From = from;
        To = to;
        Overwrote = null;
    }

    public Move(int value, Location from, Location to, int overwrote) : this(value, from, to)
    {
        Overwrote = overwrote;
    }

    // ----------------------------------------

    public override string Describe()
    {
        // [TODO] Implement this.
        throw new NotImplementedException();
    }
}
