namespace SortVisualizer.StateTracking.Actions;

/// <summary>
/// An item has been copied from one location in the visualizer to another.
/// </summary>
public record Copy : Action
{
    public readonly Location From;
    public readonly Location Dest;
    public readonly int Value;
    public readonly int? Overwrote; // value, not index

    public Copy(int value, Location from, Location dest)
    {
        Value = value;
        From = from;
        Dest = dest;
        Overwrote = null;
    }

    public Copy(SortableItem item, Location dest, int? overwrote) : this(item.Value, item.Location, dest)
    {
        Overwrote = overwrote;
    }

    public Copy(int value, Location from, Location dest, int? overwrote) : this(value, from, dest)
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
