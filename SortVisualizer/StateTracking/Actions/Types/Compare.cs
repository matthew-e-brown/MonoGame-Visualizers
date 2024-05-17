namespace SortVisualizer.StateTracking.Actions;

public enum CompareMode
{
    CompareTo,
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
    public readonly Location IndexL;
    public readonly Location IndexR;
    public readonly int ValueL;
    public readonly int ValueR;
    public readonly CompareMode Mode;

    public Compare(
        (int, Location) left,
        (int, Location) right,
        CompareMode mode = CompareMode.CompareTo
    )
    {
        (ValueL, IndexL) = left;
        (ValueR, IndexR) = right;
        Mode = mode;
    }

    // ----------------------------------------

    public override string Describe()
    {
        // [TODO] Implement this.
        throw new NotImplementedException();
    }
}
