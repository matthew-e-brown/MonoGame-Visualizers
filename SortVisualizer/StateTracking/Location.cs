namespace SortVisualizer.StateTracking;

// Just imagining all this heap allocation is making me sad. I miss Rust enums. :(

/// <summary>
/// A location where a value (as part of an operation in history) was have been accessed from or assigned to.
/// </summary>
public abstract record Location;

/// <summary>
/// This value was in the main list.
/// </summary>
/// <param name="Index">Which index this value was in.</param>
public record ArrayIndex(int Index) : Location;

/// <summary>
/// This value was in a temporary slot.
/// </summary>
/// <param name="Name">Which slot this value was in.</param>
public record TempSlot(string Name) : Location;

/// <summary>
/// The corresponding value for this action was just a regular integer; it was not part of the visualization state when
/// its operation was performed.
/// </summary>
public record OtherValue : Location;
