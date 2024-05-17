namespace SortVisualizer.StateTracking;

using SortVisualizer.StateTracking.Actions;

/// <summary>
/// A special wrapper for a plain-old integer value from inside a visualizer list.
/// <para>This wrapper is used to be able to track comparison operations between items when sorting.</para>
/// </summary>
public class SortableItem :
    IComparable<SortableItem>,
    IComparable<int>,
    IComparable,
    IEquatable<SortableItem>,
    IEquatable<int>
{
    /// <summary>
    /// What value this item actually represents.
    /// </summary>
    protected internal int Value { get; init; }

    /// <summary>
    /// Where this item currently lies in the visualizer.
    /// </summary>
    protected internal Location Location { get; set; }

    protected Visualizer Parent { get; }

    // ----------------------------------------

    protected internal SortableItem(Visualizer parent, int value, Location location)
    {
        Value = value;
        Location = location;
        Parent = parent;
    }

    protected internal SortableItem(Visualizer parent, int value, int index) : this(parent, value, new ListIndex(index))
    { }

    // ----------------------------------------

    #region Comparisons

    // This method (+overload) handles the logging of all comparisons done on the SortableItem class.

    /// <summary>
    /// Uses <c>CompareTo</c> to compare this item to another one and logs the comparison in the parent's history.
    /// </summary>
    /// <param name="other">The other item.</param>
    /// <param name="mode">Which operation was used to do the comparison.</param>
    protected int CompareUsingOperator(SortableItem other, CompareMode mode)
    {
        var left = (Value, Location);
        var right = (other.Value, other.Location);

        Parent.History.Add(new Compare(left, right, mode));

        return Value.CompareTo(other);
    }

    /// <summary>
    /// Compares this item's value to a plain integer and logs the comparison in the parent's history.
    /// </summary>
    /// <param name="value">The integer value to compare with.</param>
    /// <param name="op">Which operation was used to do the comparison.</param>
    /// <param name="swap">If true, logs the comparison as having been integer–item instead of item–integer.</param>
    protected int CompareUsingOperator(int value, CompareMode op, bool swap = false)
    {
        (int, Location) left = (Value, Location);
        (int, Location) right = (value, new FreeFloating());

        if (swap)
            (left, right) = (right, left);

        Parent.History.Add(new Compare(left, right, op));

        return Value.CompareTo(value);
    }

    // ----------------------------------------

    public int CompareTo(int other) => CompareUsingOperator(other, CompareMode.CompareTo);

    public int CompareTo(SortableItem? other)
    {
        if (other is null) throw new NullReferenceException("Attempted to compare a null SortableItem.");
        else return CompareUsingOperator(other, CompareMode.CompareTo);
    }

    // Fall back to plain equality operator for Object.Equals checks; will be logged the same as `==`.
    public bool Equals(int other) => this == other;

    public bool Equals(SortableItem? other)
    {
        if (other is null) throw new NullReferenceException("Attempted to compare a null SortableItem.");
        else return this == other;
    }

    public int CompareTo(object? obj)
    {
        if (obj is SortableItem other) return CompareUsingOperator(other, CompareMode.CompareTo);
        else if (obj is int value) return CompareUsingOperator(value, CompareMode.CompareTo);
        else if (obj is null) throw new NullReferenceException("Attempted to compare a null object to a SortableItem.");
        else throw new ArgumentException("Object is not a SortableItem");
    }

    public override bool Equals(object? obj)
    {
        if (obj is SortableItem other) return Equals(other);
        else if (obj is int value) return Equals(value);
        else if (obj is null) throw new NullReferenceException("Attempted to compare a null object to a SortableItem.");
        else throw new ArgumentException("Object is not a SortableItem");
    }

    // ----------------------------------------

    #region Comparison operators

    // Allow comparison with self
    public static bool operator ==(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.Equal) == 0;
    public static bool operator !=(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.NotEqual) != 0;
    public static bool operator <=(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.LessThanOrEqual) <= 0;
    public static bool operator >=(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.GreaterThanOrEqual) >= 0;
    public static bool operator <(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.LessThan) < 0;
    public static bool operator >(SortableItem a, SortableItem b) => a.CompareUsingOperator(b, CompareMode.GreaterThan) > 0;

    // Allow comparison with an integer
    public static bool operator ==(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.Equal) == 0;
    public static bool operator !=(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.NotEqual) != 0;
    public static bool operator <=(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.LessThanOrEqual) <= 0;
    public static bool operator >=(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.GreaterThanOrEqual) >= 0;
    public static bool operator <(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.LessThan) < 0;
    public static bool operator >(SortableItem a, int b) => a.CompareUsingOperator(b, CompareMode.GreaterThan) > 0;

    // Same comparisons, but with integer first
    public static bool operator ==(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.Equal, swap: true) == 0;
    public static bool operator !=(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.NotEqual, swap: true) != 0;
    public static bool operator <=(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.LessThanOrEqual, swap: true) <= 0;
    public static bool operator >=(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.GreaterThanOrEqual, swap: true) >= 0;
    public static bool operator <(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.LessThan, swap: true) < 0;
    public static bool operator >(int a, SortableItem b) => b.CompareUsingOperator(a, CompareMode.GreaterThan, swap: true) > 0;

    #endregion

    // ----------------------------------------

    #endregion

    // ----------------------------------------

    // Hash as if this item was just the integer value
    public override int GetHashCode() => Value.GetHashCode();

    // Also print the item as if it was just a string.
    public override string ToString() => Value.ToString();
}
