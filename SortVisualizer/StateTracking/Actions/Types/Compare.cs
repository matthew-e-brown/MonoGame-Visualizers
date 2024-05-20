namespace SortVisualizer.StateTracking.Actions;

/// <summary>
/// The different ways two <see cref="SortableItem" />s may be compared with one another (or with an integer).
/// </summary>
public enum CompareMode
{
    CompareTo,
    /// <remarks>
    /// Includes both the <c cref="SortableItem.Equals(object?)">Equals</c> method and the <c
    /// cref="SortableItem.op_Equality">==</c> operator.
    /// </remarks>
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}

/// <summary>
/// Two items have been compared with one another.
/// </summary>
public record Compare : Action
{
    public readonly Location LocationA;
    public readonly Location LocationB;
    public readonly int ValueA;
    public readonly int ValueB;
    public readonly CompareMode Mode;

    public Compare(int valueA, Location locationA, int valueB, Location locationB, CompareMode mode)
    {
        LocationA = locationA;
        LocationB = locationB;
        ValueA = valueA;
        ValueB = valueB;
        Mode = mode;
    }

    public Compare(SortableItem a, SortableItem b, CompareMode mode) : this(a.Value, a.Location, b.Value, b.Location, mode)
    { }

    public Compare(SortableItem a, int b, CompareMode mode) : this(a.Value, a.Location, b, new OtherValue(), mode)
    { }

    public Compare(int a, SortableItem b, CompareMode mode) : this(a, new OtherValue(), b.Value, b.Location, mode)
    { }

    // ----------------------------------------

    public override string Describe()
    {
        // [TODO] Implement this.
        throw new NotImplementedException();
    }
}
