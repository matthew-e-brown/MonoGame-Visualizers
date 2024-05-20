namespace SortVisualizer.StateTracking;

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
    /// <summary>
    /// The main set of items being sorted.
    ///
    /// <para>
    /// These items are represented in the visualizer as boxes of varying heights.
    /// </para>
    /// </summary>
    protected internal SortableItem[] MainArray { get; }

    /// <summary>
    /// Temporary slots off to the side of the main array in the visualizer, tracked by name (as if they were a literal
    /// `int temp = ...` in the user's algorithm).
    ///
    /// <para>
    /// The heights of items in temp slots are marked with a thin dotted line that spans the width of the visualizer.
    /// This is so they can be compared with the other items in the array.
    /// </para>
    /// </summary>
    protected internal Dictionary<string, SortableItem> TempSlots { get; }

    /// <summary>
    /// Specific indices that the user has marked as being relevant for some reason or another.
    ///
    /// <para>
    /// These indices are marked with arrows pointing down at the slot.
    /// </para>
    /// </summary>
    protected internal Dictionary<string, (int index, Color color)> MarkedIndices { get; }

    /// <summary>
    /// Similar to <c cref="MarkedIndices">MarkedIndices</c>, but for entire ranges instead of individual locations.
    ///
    /// <para>
    /// These ranges are represented both by highlights on the boxes themselves and by coloured lines underneath the
    /// array.
    /// </para>
    /// </summary>
    protected internal Dictionary<string, (Range range, Color color)> MarkedRanges { get; }

    /// <summary>
    /// A list of all of the <see cref="Action">actions</see> that the user has performed thus far during the execution
    /// of their sorting algorithm.
    /// </summary>
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
    /// Swaps the locations of the two given <c cref="SortableItem">SortableItem</c>s.
    ///
    /// <para>
    /// This will also swap the references in parameters <paramref name="a"/> and <paramref name="b"/>.
    /// </para>
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

    #region Range copying

    /// <summary>
    /// Takes a range of <paramref name="count"/> items starting at <paramref name="index"/> and moves it left or right
    /// by <paramref name="offset"/> slots.
    ///
    /// <para>
    /// Whatever items were present in the destination region are <b>overwritten.</b> Use <see
    /// cref="this[string]">temp-slots</see> to preserve them.
    /// </para>
    /// <para>
    /// This method does nothing if <paramref name="count"/> or <paramref name="offset"/> are zero.
    /// </para>
    /// </summary>
    ///
    /// <param name="index">The start of the range to copy.</param>
    /// <param name="count">How many items should be copied.</param>
    /// <param name="offset">How far and in which direction the items should be copied within the array. Can be positive
    /// or negative.</param>
    ///
    /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is outside the bounds of the
    /// array.</exception>
    /// <exception cref="ArgumentException">If any part of the destination range falls outside the array.</exception>
    public void CopyRange(int index, int count, int offset)
    {
        if (!(0 < index && index < MainArray.Length))
            throw new IndexOutOfRangeException("Index was outside the bounds of the array.");
        else if (index + offset < 0)
            throw new ArgumentException("Start of destination range is outside the bounds of the array.");
        else if (index + count + offset >= MainArray.Length)
            throw new ArgumentException("End of destination range is outside the bounds of the array.");

        if (offset == 0 || count == 0)
            return;

        // Shifting by `offset` in either direction means overwriting a count of `offset` values. The added cost of
        // saving which values are overwritten is simply to allow the animation to undo/redo without losing values that
        // got replaced.
        int[] overwritten = new int[Math.Abs(offset)];

        // End points are exclusive. The range we want to preserve is the region of `dst` which does not intersect with
        // `src`. It starts where `dst` starts if `dst` is entirely after the end of `src`, otherwise it starts after
        // `src` ends. It ends either where `dst` ends or where `src` begins, whichever is first.
        var (srcStart, srcEnd) = (index, index + count);
        var (dstStart, dstEnd) = (srcStart + offset, srcEnd + offset);

        // e.g: index = 3, count = 2, offset = +4 --> copying [3, 4]          into [7, 8]          ==> Save [7, 8].
        // e.g: index = 1, count = 5, offset = +2 --> copying [1, 2, 3, 4, 5] into [3, 4, 5, 6, 7] ==> Save [6, 7].
        // e.g: index = 4, count = 5, offset = -1 --> copying [4, 5, 6, 7, 8] into [3, 4, 5, 6, 7] ==> Save [3].
        var saveStart = Math.Max(srcEnd + 1, dstStart);
        var saveEnd = Math.Min(dstEnd, srcStart);

        for (int i = saveStart; i < saveEnd; i++)
            overwritten[i - saveStart] = MainArray[i].Value;

        // Now we can actually do the copy.
        Array.Copy(MainArray, srcStart, MainArray, dstStart, count);

        // Finally, go and tell all the items that we've moved them and push the action into history.
        for (int i = dstStart; i < dstEnd; i++)
            MainArray[i].Location = new ArrayIndex(i);

        History.Add(new CopyRange(index, count, offset, overwritten));
    }

    #endregion

    // ----------------------------------------
}
