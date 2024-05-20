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
    /// What value this item actually represents (the height of its box in the visualizer). This value is internal; the
    /// end-user cannot see it. Instead, they can compare this object directly with an integer (that operation will be
    /// tracked).
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Where this item currently lies in the visualizer.
    /// </summary>
    protected internal Location Location;

    /// <summary>
    /// A pointer to the parent visualizer to allow this item to insert things into history.
    /// </summary>
    protected internal readonly TrackedArray Parent;

    // ----------------------------------------

    #region Constructors

    /// <summary>
    /// Creates a brand new <c>SortableItem</c> at a given location.
    /// </summary>
    /// <param name="parent">Which visualizer this item belongs to.</param>
    /// <param name="value">What integer value this item represents.</param>
    /// <param name="location">Where in the visualizer this item is located.</param>
    protected internal SortableItem(TrackedArray parent, int value, Location location)
    {
        Value = value;
        Location = location;
        Parent = parent;
    }

    /// <summary>
    /// Creates a new <c>SortableItem</c> with the same value as an old one in a new location.
    /// </summary>
    /// <param name="oldItem">The <c>SortableItem</c> to copy.</param>
    /// <param name="newLocation">Where this new item is located in the visualizer.</param>
    protected internal SortableItem(SortableItem oldItem, Location newLocation) : this(oldItem.Parent, oldItem.Value, newLocation)
    { }

    /// <summary>
    /// Creates a brand new <c>SortableItem</c> at a given index in the main visualizer array.
    /// </summary>
    /// <param name="parent">Which visualizer this item belongs to.</param>
    /// <param name="value">What integer value this item represents.</param>
    /// <param name="index">Which index in the main array this item is being placed.</param>
    protected internal SortableItem(TrackedArray parent, int value, int index) : this(parent, value, new ArrayIndex(index))
    { }

    /// <summary>
    /// Creates a new <c>SortableItem</c> with the same value as an old one at a given index in the main visualizer
    /// array.
    /// </summary>
    /// <param name="oldItem">The <c>SortableItem</c> to copy.</param>
    /// <param name="newIndex">Where in the main array this item is being placed.</param>
    protected internal SortableItem(SortableItem oldItem, int newIndex) : this(oldItem, new ArrayIndex(newIndex))
    { }

    /// <summary>
    /// Creates a brand new <c>SortableItem</c> in a given temporary slot.
    /// </summary>
    /// <param name="parent">Which visualizer this item belongs to.</param>
    /// <param name="value">What integer value this item represents.</param>
    /// <param name="slotName">Which of the temporary slots to insert this item into.</param>
    protected internal SortableItem(TrackedArray parent, int value, string slotName) : this(parent, value, new TempSlot(slotName))
    { }

    /// <summary>
    /// Creates a new <c>SortableItem</c> with the same value as an old one in a specific temporary slot.
    /// </summary>
    /// <param name="oldItem">The <c>SortableItem</c> to copy.</param>
    /// <param name="slotName">The name of the temporary slot where this item is being placed.</param>
    protected internal SortableItem(SortableItem oldItem, string slotName) : this(oldItem, new TempSlot(slotName))
    { }

    #endregion

    // ----------------------------------------

    #region Comparisons

    // ----------------------------------------

    // This method and its overloads handle all the actual comparisons. The rest just forward here.

    protected static int CompareAndLog(SortableItem a, SortableItem b, CompareMode mode)
    {
        if (!ReferenceEquals(a.Parent, b.Parent))
            throw new ArgumentException("Cannot compare SortableItems from two different visualizers.");

        int result = a.Value.CompareTo(b.Value);
        a.Parent.History.Add(new Compare(a, b, mode));
        return result;
    }

    protected static int CompareAndLog(SortableItem a, int b, CompareMode mode)
    {
        int result = a.Value.CompareTo(b);
        a.Parent.History.Add(new Compare(a, b, mode));
        return result;
    }

    protected static int CompareAndLog(int a, SortableItem b, CompareMode mode)
    {
        int result = a.CompareTo(b.Value);
        b.Parent.History.Add(new Compare(a, b, mode));
        return result;
    }

    // ----------------------------------------

    #region Interface implementations

    public int CompareTo(int other) => CompareAndLog(this, other, CompareMode.CompareTo);

    public int CompareTo(SortableItem? other)
    {
        if (other is null) throw new NullReferenceException("Attempted to compare a null SortableItem.");
        else return CompareAndLog(this, other, CompareMode.CompareTo);
    }

    public int CompareTo(object? obj)
    {
        if (obj is SortableItem other) return CompareAndLog(this, other, CompareMode.CompareTo);
        else if (obj is int value) return CompareAndLog(this, value, CompareMode.CompareTo);
        else if (obj is null) throw new NullReferenceException("Attempted to compare a null object to a SortableItem.");
        else throw new ArgumentException("Object is not a SortableItem");
    }

    // ----------------------------------------

    public bool Equals(int other) => CompareAndLog(this, other, CompareMode.Equal) == 0;

    public bool Equals(SortableItem? other)
    {
        if (other is null) throw new NullReferenceException("Attempted to compare a null SortableItem.");
        else return CompareAndLog(this, other, CompareMode.Equal) == 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is SortableItem other) return Equals(other);
        else if (obj is int value) return Equals(value);
        else if (obj is null) throw new NullReferenceException("Attempted to compare a null object to a SortableItem.");
        else throw new ArgumentException("Object is not a SortableItem");
    }

    #endregion

    // ----------------------------------------

    #region Comparison operators

    // Allow comparison with self
    public static bool operator ==(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.Equal) == 0;
    public static bool operator !=(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.NotEqual) != 0;
    public static bool operator <=(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.LessThanOrEqual) <= 0;
    public static bool operator >=(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.GreaterThanOrEqual) >= 0;
    public static bool operator <(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.LessThan) < 0;
    public static bool operator >(SortableItem a, SortableItem b) => CompareAndLog(a, b, CompareMode.GreaterThan) > 0;

    // Allow comparison with an integer
    public static bool operator ==(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.Equal) == 0;
    public static bool operator !=(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.NotEqual) != 0;
    public static bool operator <=(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.LessThanOrEqual) <= 0;
    public static bool operator >=(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.GreaterThanOrEqual) >= 0;
    public static bool operator <(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.LessThan) < 0;
    public static bool operator >(SortableItem a, int b) => CompareAndLog(a, b, CompareMode.GreaterThan) > 0;

    // Same comparisons, but with integer first
    public static bool operator ==(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.Equal) == 0;
    public static bool operator !=(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.NotEqual) != 0;
    public static bool operator <=(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.LessThanOrEqual) <= 0;
    public static bool operator >=(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.GreaterThanOrEqual) >= 0;
    public static bool operator <(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.LessThan) < 0;
    public static bool operator >(int a, SortableItem b) => CompareAndLog(a, b, CompareMode.GreaterThan) > 0;

    #endregion

    // ----------------------------------------

    #endregion

    // ----------------------------------------

    // Print this item as if it was just its integer value.
    public override string ToString() => Value.ToString();

    // Hash this item as if it was just its integer value.
    public override int GetHashCode() => Value.GetHashCode();
}
