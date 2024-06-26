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

    /// <summary>
    /// Gets or sets which <c cref="SortableItem">SortableItem</c> is currently in the main visualizer array at the
    /// given index.
    /// </summary>
    /// <param name="arrayIndex">The index of the item to get or set.</param>
    /// <returns>A reference to the item currently at the given index in the main visualizer array.</returns>
    public SortableItem this[int arrayIndex]
    {
        get
        {
            if (!(0 <= arrayIndex && arrayIndex < MainArray.Length))
                throw new AlgoIndexException("get index", arrayIndex, MainArray.Length);
            return MainArray[arrayIndex];
        }

        set
        {
            if (!(0 <= arrayIndex && arrayIndex < MainArray.Length))
                throw new AlgoIndexException("set index", arrayIndex, MainArray.Length);

            var newLoc = new ArrayIndex(arrayIndex);
            int oldVal = MainArray[arrayIndex].Value;

            MainArray[arrayIndex] = value;
            MainArray[arrayIndex].Location = newLoc;

            // Only add to history if indexing was successful.
            History.Add(new Copy(value, newLoc, oldVal));
        }
    }

    /// <summary>
    /// Gets or sets which <c cref="SortableItem">SortableItem</c> is currently in a temp-slot on the side of the main
    /// visualizer array.
    /// </summary>
    /// <param name="slotName">The name of the slot to get an item from or put an item into.</param>
    /// <returns>
    /// A reference to the item currently in the given temp-slot. If there is no item in the slot, a get operation will
    /// throw an <see cref="AlgoSlotException">exception</see>; a set operation will insert it there.
    /// </returns>
    public SortableItem this[string slotName]
    {
        get
        {
            if (!TempSlots.TryGetValue(slotName, out SortableItem? value))
                throw new AlgoSlotException("get temp-slot", slotName);
            return value;
        }

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
    public void Swap(int i, int j)
    {
        if (!(0 <= i && i < MainArray.Length)) throw new AlgoIndexException("swap", i, MainArray.Length);
        if (!(0 <= j && j < MainArray.Length)) throw new AlgoIndexException("swap", j, MainArray.Length);

        History.Add(new Swap(MainArray[i], MainArray[j]));

        // Move A from `i` into `j`, but also make sure it knows that it now lives at `j`.
        (MainArray[i], MainArray[j]) = (MainArray[j], MainArray[i]);
        MainArray[i].Location = new ArrayIndex(i);
        MainArray[j].Location = new ArrayIndex(j);
    }

    /// <summary>
    /// Swaps the item in the array at index <paramref name="arrayIdx"/> with the one currently in temp-slot <paramref
    /// name="slotName"/>.
    /// </summary>
    /// <param name="arrayIdx">The index of the item to swap into the temp-slot / destination of the second item.</param>
    /// <param name="slotName">The slot to swap the first item into / second item out of.</param>
    public void Swap(int arrayIdx, string slotName) => Swap(arrayIdx, slotName, reverseOrder: false);

    /// <summary>
    /// Swaps the item currently in temp-slot <paramref name="slotName"/> with the one in the array at index <paramref
    /// name="arrayIdx"/>.
    /// </summary>
    /// <param name="slotName">The slot to swap the second item into / first item out of.</param>
    /// <param name="arrayIdx">The index of the item to swap into the temp-slot / destination of the first item.</param>
    public void Swap(string slotName, int arrayIdx) => Swap(arrayIdx, slotName, reverseOrder: true);

    /// <summary>
    /// Internal implementation of both int/string swaps.
    /// </summary>
    /// <param name="idx">The index to swap to/from.</param>
    /// <param name="slotName">The slot to swap to/from.</param>
    /// <param name="reverseOrder">Whether or not the history item should display "slot⇄index" or "index⇄slot". The
    /// latter is the default.</param>
    private void Swap(int idx, string slotName, bool reverseOrder)
    {
        if (!(0 <= idx && idx < MainArray.Length)) throw new AlgoIndexException("swap", idx, MainArray.Length);
        else if (!TempSlots.ContainsKey(slotName)) throw new AlgoSlotException("swap", slotName);

        var action = reverseOrder
            ? new Swap(TempSlots[slotName], MainArray[idx])
            : new Swap(MainArray[idx], TempSlots[slotName]);
        History.Add(action);

        (MainArray[idx], TempSlots[slotName]) = (TempSlots[slotName], MainArray[idx]);
        MainArray[idx].Location = new ArrayIndex(idx);
        TempSlots[slotName].Location = new TempSlot(slotName);
    }

    /// <summary>
    /// Swaps the items two temporary slots.
    /// </summary>
    /// <param name="slot1">Name of the first slot to swap to/from.</param>
    /// <param name="slot2">Name of the second slot to swap to/from.</param>
    public void Swap(string slot1, string slot2)
    {
        if (!TempSlots.ContainsKey(slot1)) throw new AlgoSlotException("swap", slot1);
        if (!TempSlots.ContainsKey(slot2)) throw new AlgoSlotException("swap", slot2);

        History.Add(new Swap(TempSlots[slot1], TempSlots[slot2]));

        (TempSlots[slot1], TempSlots[slot2]) = (TempSlots[slot2], TempSlots[slot1]);
        TempSlots[slot1].Location = new TempSlot(slot1);
        TempSlots[slot2].Location = new TempSlot(slot2);
    }

    /// <summary>
    /// Swaps the locations of the two given <c cref="SortableItem">SortableItem</c>s.
    ///
    /// <para>
    /// This will also swap the references in parameters <paramref name="a"/> and <paramref name="b"/>.
    /// </para>
    /// </summary>
    public static void Swap(ref SortableItem a, ref SortableItem b)
    {
        if (!ReferenceEquals(a.Parent, b.Parent))
            throw new ArgumentException("Cannot swap SortableItems from two different visualizers.");

        a.Parent.History.Add(new Swap(a, b));
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
    public void CopyRange(int index, int count, int offset)
    {
        // End points are exclusive.
        var (srcStart, srcEnd) = (index, index + count);
        var (dstStart, dstEnd) = (srcStart + offset, srcEnd + offset);

        if (!(0 < srcStart && srcStart < MainArray.Length))
            throw new AlgoIndexException("copy range", srcStart, MainArray.Length);
        else if (srcEnd >= MainArray.Length)
            throw new AlgoIndexException("copy range", srcEnd, MainArray.Length, "range extends past the bounds of the array.");
        else if (dstStart < 0)
            throw new AlgoIndexException("copy range", dstStart, MainArray.Length, "start of destination range would be outside the bounds of the array.");
        else if (dstEnd >= MainArray.Length)
            throw new AlgoIndexException("copy range", dstEnd, MainArray.Length, "end of destination range would be outside the bounds of the array.");

        if (offset == 0 || count == 0)
            return;

        // ----------------------------------------

        // Shifting by `offset` in either direction means overwriting a count of `offset` values. The added cost of
        // saving which values are overwritten is simply to allow the animation to undo/redo without losing values that
        // got replaced.
        int[] overwritten = new int[Math.Abs(offset)];

        // The range we want to preserve is the region of `dst` which does not intersect with `src`. It starts where
        // `dst` starts if `dst` is entirely after the end of `src`, otherwise it starts after `src` ends. It ends
        // either where `dst` ends or where `src` begins, whichever is first.

        // e.g: index = 3, count = 2, offset = +4 --> copying [3, 4]          into [7, 8]          ==> Save [7, 8].
        // e.g: index = 1, count = 5, offset = +2 --> copying [1, 2, 3, 4, 5] into [3, 4, 5, 6, 7] ==> Save [6, 7].
        // e.g: index = 4, count = 5, offset = -1 --> copying [4, 5, 6, 7, 8] into [3, 4, 5, 6, 7] ==> Save [3].

        var saveStart = Math.Max(srcEnd + 1, dstStart);
        var saveEnd = Math.Min(dstEnd, srcStart);

        for (int i = saveStart; i < saveEnd; i++)
            overwritten[i - saveStart] = MainArray[i].Value;

        // ----------------------------------------

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
