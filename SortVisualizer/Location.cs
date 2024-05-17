namespace SortVisualizer;

// Just imagining all this heap allocation is making me sad. I miss Rust enums. :(

/// <summary>
/// A location where a value (as part of an operation in history) was have been accessed from or assigned to.
/// </summary>
public abstract record Location
{
    /// <summary>
    /// Casts an integer into a <c cref="ListIndex">list index</c> location.
    /// </summary>
    public static explicit operator Location(int index) => new ListIndex(index);
}

/// <summary>
/// This value was not part of the visualization state when its operation was performed.
/// </summary>
public record FreeFloating : Location;

/// <summary>
/// This value was in the main list.
/// </summary>
/// <param name="Index">Which index this value was in.</param>
public record ListIndex(int Index) : Location;

/// <summary>
/// This value was in a temporary slot.
/// </summary>
/// <param name="Name">Which slot this value was in.</param>
public record TempSlot(string Name) : Location;
