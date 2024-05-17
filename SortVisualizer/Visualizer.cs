namespace SortVisualizer;

using SortVisualizer.Actions;
using Microsoft.Xna.Framework;


public class Visualizer
{
    public ArrayProxy Items { get; }

    // [TODO] Consideration to make: do we make TempSlots be another tracked class? That way the user could assign to
    // slots directly.
    protected internal Dictionary<string, SortableItem> TempSlots { get; }

    protected internal Dictionary<string, (int value, Color color)> MarkedHeights { get; }
    protected internal Dictionary<string, (int index, Color color)> MarkedIndices { get; }
    protected internal Dictionary<string, (Range range, Color color)> MarkedRanges { get; }

    protected internal List<Action> History { get; }

    // ----------------------------------------

    public Visualizer(IEnumerable<int> startingValues)
    {
        Items = new ArrayProxy(this, startingValues);

        TempSlots = new();
        MarkedHeights = new();
        MarkedIndices = new();
        MarkedRanges = new();

        History = new();
    }
}
