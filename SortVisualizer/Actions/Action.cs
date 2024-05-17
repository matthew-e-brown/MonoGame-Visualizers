namespace SortVisualizer.Actions;

/// <summary>
/// An operation that the user has performed on the list of values throughout the sort.
/// </summary>
public abstract record Action
{
    /// <summary>
    /// Returns a string describing this action to the user.
    /// <para>
    /// This string is the value that gets drawn on-screen in the visualizer, or in the console, when explaining the
    /// visualizer's current step to the user.
    /// </para>
    /// </summary>
    public abstract string Describe();

    /// <summary>
    /// Applies this action to the renderer's current state, allowing it to advance a frame.
    /// </summary>
    /// <param name="values"></param>
    protected internal virtual void Apply(int[] values)
    {
        // Actions do not need to mutate the visualizer state by default.
    }

    /// <summary>
    /// Undoes the modifications performed by this action so that the rendered visualization may rewind.
    /// </summary>
    /// <param name="values"></param>
    protected internal virtual void Undo(int[] values)
    {
        // Actions do not need to mutate the visualizer state by default.
    }
}
