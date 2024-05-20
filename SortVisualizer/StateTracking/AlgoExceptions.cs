namespace SortVisualizer.StateTracking;

/// <summary>
/// An exception thrown due to an error in the user's visualized algorithm, such as array-out-of-bounds or key-not-found
/// errors.
/// </summary>
public class AlgoException : Exception
{
    public AlgoException()
    { }

    public AlgoException(string message) : base(message)
    { }
}

/// <summary>
/// The user attempted to access an array element which is out of bounds.
/// </summary>
public class AlgoIndexException : AlgoException
{
    public int Index { get; }
    public int ArrayLength { get; }
    public string AttemptedAction { get; }

    public AlgoIndexException(string action, int index, int length) : base($"Failed to {action}: index {index} is out of bounds.")
    {
        Index = index;
        ArrayLength = length;
        AttemptedAction = action;
    }

    public AlgoIndexException(string action, int index, int length, string message) : base($"Failed to {action}: {message}")
    {
        Index = index;
        ArrayLength = length;
        AttemptedAction = action;
    }
}

/// <summary>
/// The user attempted to access a temp-slot which did not exist.
/// </summary>
public class AlgoSlotException : AlgoException
{
    public string SlotName { get; }
    public string AttemptedAction { get; }

    public AlgoSlotException(string action, string slotName) : base($"Failed to {action}: temp-slot '{slotName}' does not exist.")
    {
        SlotName = slotName;
        AttemptedAction = action;
    }
}
