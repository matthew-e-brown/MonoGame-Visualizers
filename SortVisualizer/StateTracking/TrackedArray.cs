namespace SortVisualizer;

using SortVisualizer.StateTracking;
using SortVisualizer.StateTracking.Actions;
using Microsoft.Xna.Framework;

/// <summary>
/// A collections of <c cref="SortableItem">SortableItem</c>s which tracks things like moves, swaps, inserts, and more,
/// in order to create an animation.
/// </summary>
///
/// <remarks>
/// <para>
/// This class has an indexer for <c>int</c>; you can treat it just like an array to access the items to be sorted.
/// There is also an indexer for <c>string</c>, which can be used to create temporary variables.
/// </para>
/// </remarks>
public class TrackedArray
{
    protected internal SortableItem[] MainArray { get; }

    /// <summary>
    /// Items that are off the side of the main array. Their heights will be marked with a thin dotted line that spans
    /// the width of the visualizer, allowing for easy visual comparisons.
    /// </summary>
    protected internal Dictionary<string, SortableItem> TempSlots { get; }

    protected internal Dictionary<string, (int index, Color color)> MarkedIndices { get; }
    protected internal Dictionary<string, (Range range, Color color)> MarkedRanges { get; }

    protected internal List<Action> History { get; }

    // ----------------------------------------

    public TrackedArray(IEnumerable<int> startingValues)
    {
        MainArray = startingValues
            .Select((value, index) => new SortableItem(this, value, index))
            .ToArray();

        TempSlots = new();
        MarkedIndices = new();
        MarkedRanges = new();

        History = new();
    }

    // ----------------------------------------

    #region Indexing

    // When an item is retrieved from the visualizer, return it as-is, since they have their own current location as a
    // property. When it gets inserted back somewhere else, we'll know where it came from.

    public SortableItem this[int arrayIndex]
    {
        get => MainArray[arrayIndex];
        set
        {
            int oldVal;
            var newLoc = new ArrayIndex(arrayIndex);

            try
            {
                oldVal = MainArray[arrayIndex].Value;
                MainArray[arrayIndex] = value;
                MainArray[arrayIndex].Location = newLoc;
            }
            catch (IndexOutOfRangeException)
            {
                throw; // Catch and re-throw just for the sake of explicitness.
            }

            // Only add to history if indexing was successful.
            History.Add(new Copy(value, newLoc, oldVal));
        }
    }

    public SortableItem this[string slotName]
    {
        get => TempSlots[slotName];
        set
        {
            var newLoc = new TempSlot(slotName);
            var oldVal = TempSlots.GetValueOrDefault(slotName)?.Value; // May be null

            // This one doesn't need a try-catch because assigning to a slot when there's nothing there will just create
            // an entry.
            TempSlots[slotName] = value;
            TempSlots[slotName].Location = newLoc;

            History.Add(new Copy(value, newLoc, oldVal));
        }
    }

    #endregion

    // ----------------------------------------

    #region Swapping

    /// <summary>
    /// Swaps the two items living at indices <paramref name="i"/> and <paramref name="j"/> in the main visualizer list.
    /// </summary>
    /// <param name="i">Index of the first item to swap.</param>
    /// <param name="j">Index of the second item to swap.</param>
    /// <exception cref="IndexOutOfRangeException">If either of the given indices are outside the bounds of the
    /// array.</exception>
    public void Swap(int i, int j)
    {
        Action action;

        try
        {
            action = new Swap(MainArray[i], MainArray[j]);

            // Move A from `i` into `j`, but also make sure it knows that it now lives at `j`.
            (MainArray[i], MainArray[j]) = (MainArray[j], MainArray[i]);
            MainArray[i].Location = new ArrayIndex(i);
            MainArray[j].Location = new ArrayIndex(j);
        }
        catch (IndexOutOfRangeException)
        {
            throw; // Again, just for the sake of explicitness.
        }

        // Only add to history if indexing was successful.
        History.Add(action);
    }

    /// <summary>
    /// Swaps the item in the array at index <paramref name="arrayIdx"/> with the one currently in temp-slot <paramref
    /// name="slotName"/>.
    /// </summary>
    /// <param name="arrayIdx">The index of the item to swap into the temp-slot / destination of the second item.</param>
    /// <param name="slotName">The slot to swap the first item into / second item out of.</param>
    /// <exception cref="IndexOutOfRangeException">If the given index is outside the bounds of the array.</exception>
    /// <exception cref="KeyNotFoundException">If the given temp-slot doesn't have anything in it yet.</exception>
    public void Swap(int arrayIdx, string slotName) => Swap(arrayIdx, slotName, reverseOrder: false);

    /// <summary>
    /// Swaps the item currently in temp-slot <paramref name="slotName"/> with the one in the array at index <paramref
    /// name="arrayIdx"/>.
    /// </summary>
    /// <param name="slotName">The slot to swap the second item into / first item out of.</param>
    /// <param name="arrayIdx">The index of the item to swap into the temp-slot / destination of the first item.</param>
    /// <exception cref="KeyNotFoundException">If the given temp-slot doesn't have anything in it yet.</exception>
    /// <exception cref="IndexOutOfRangeException">If the given index is outside the bounds of the array.</exception>
    public void Swap(string slotName, int arrayIdx) => Swap(arrayIdx, slotName, reverseOrder: true);

    /// <summary>
    /// Internal implementation of both int/string swaps.
    /// </summary>
    /// <param name="arrayIdx">The index to swap to/from.</param>
    /// <param name="slotName">The slot to swap to/from.</param>
    /// <param name="reverseOrder">Whether or not the history item should display "slot⇄index" or "index⇄slot". The
    /// latter is the default.</param>
    private void Swap(int arrayIdx, string slotName, bool reverseOrder)
    {
        Action action;

        try
        {
            action = reverseOrder
                ? new Swap(TempSlots[slotName], MainArray[arrayIdx])
                : new Swap(MainArray[arrayIdx], TempSlots[slotName]);

            (MainArray[arrayIdx], TempSlots[slotName]) = (TempSlots[slotName], MainArray[arrayIdx]);
            MainArray[arrayIdx].Location = new ArrayIndex(arrayIdx);
            TempSlots[slotName].Location = new TempSlot(slotName);
        }
        catch (IndexOutOfRangeException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }

        // Only add to history if indexing was successful.
        History.Add(action);
    }

    /// <summary>
    /// Swaps the items two temporary slots.
    /// </summary>
    /// <param name="slot1">Name of the first slot to swap to/from.</param>
    /// <param name="slot2">Name of the second slot to swap to/from.</param>
    /// <exception cref="KeyNotFoundException">If either of the two temp-slots do not have anything in them
    /// yet.</exception>
    public void Swap(string slot1, string slot2)
    {
        Action action;

        try
        {
            action = new Swap(TempSlots[slot1], TempSlots[slot2]);

            (TempSlots[slot1], TempSlots[slot2]) = (TempSlots[slot2], TempSlots[slot1]);
            TempSlots[slot1].Location = new TempSlot(slot1);
            TempSlots[slot2].Location = new TempSlot(slot2);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }

        // Only add to history if indexing was successful.
        History.Add(action);
    }

    /// <summary>
    /// Swaps the positions of <c cref="SortableItem">SortableItem</c>s <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    public void Swap(ref SortableItem a, ref SortableItem b)
    {
        History.Add(new Swap(a, b));
        var aOldLoc = a.Location;
        var bOldLoc = b.Location;

        // Swap them as usual, but ensure they know what their new positions are.
        (a, b) = (b, a);
        a.Location = bOldLoc;
        b.Location = aOldLoc;
    }

    #endregion

    // ----------------------------------------
}
