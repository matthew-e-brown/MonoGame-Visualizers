namespace SortVisualizer.StateTracking.Actions;

/// <summary>
/// A range of items has been copied to the left or to the right.
/// </summary>
public record CopyRange : Action
{
    public readonly int Index;
    public readonly int Count;
    public readonly int Offset; // +/-

    /// <summary>
    /// An array of <c cref="Count">Count</c> elements from either side of the shifted range that contains the values of
    /// what was overwritten by the shift.
    /// </summary>
    /// <remarks>
    /// This is necessary to keep as a property because it allows us to undo/redo this operation without totally losing
    /// the values that were overwritten.
    /// </remarks>
    public readonly int[] Overwrote;

    public CopyRange(int index, int count, int offset, int[] overwrote)
    {
        Index = index;
        Count = count;
        Offset = offset;
        Overwrote = overwrote;
    }

    // ----------------------------------------

    // [TODO] Implement these.

    public override string Describe()
    {
        throw new NotImplementedException();
    }

    protected internal override void Apply(int[] values)
    {
        throw new NotImplementedException();
    }

    protected internal override void Undo(int[] values)
    {
        throw new NotImplementedException();
    }
}
