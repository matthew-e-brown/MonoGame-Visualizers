namespace SortVisualizer.StateTracking.Actions;

public record Swap : Action
{
    public readonly int ValueL;
    public readonly int ValueR;
    public readonly Location LocationL;
    public readonly Location LocationR;

    public Swap(int valueL, Location locationL, int valueR, Location locationR)
    {
        ValueL = valueL;
        ValueR = valueR;
        LocationL = locationL;
        LocationR = locationR;
    }

    public Swap(SortableItem a, SortableItem b) : this(a.Value, a.Location, b.Value, b.Location)
    { }

    // ----------------------------------------

    public override string Describe()
    {
        // [TODO] Implement this.
        throw new NotImplementedException();
    }
}
