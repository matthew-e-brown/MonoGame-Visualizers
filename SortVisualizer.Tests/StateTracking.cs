namespace SortVisualizer.StateTracking.Tests;

using SortVisualizer.StateTracking;
using SortVisualizer.StateTracking.Actions;


/// <summary>
/// A wrapper for the <c cref="TrackedArray">TrackedArray</c> class that has public <see
/// cref="TrackedArray.History">history</see>.
/// </summary>
internal class ArrayWrapper : TrackedArray
{
    public new List<Action> History { get => base.History; }

    public ArrayWrapper(IEnumerable<int> startingValues) : base(startingValues)
    { }
}


/// <summary>
/// Tests for the <c>StateTracker</c> namespace.
/// </summary>
[TestClass]
public class StateTrackingTests
{
    static readonly int[] starterValues = new[] { 8, 50, 22, 98, 41, 78, 3, 50, 92, 14 };

    /// <summary>
    /// Gets all the indices worth comparing for <c cref="starterValues">starterValues</c>.
    /// </summary>
    /// <returns>
    /// All indices in the range <i>(0, 0), (0, 1), ..., (1, 0), (1, 1), ... (n, n)</i>.
    /// </returns>
    private static IEnumerable<(int, int)> StarterIndicesCartesian()
    {
        for (int i = 0; i < starterValues.Length; i++)
            for (int j = 0; j < starterValues.Length; j++)
                yield return (i, j);
    }

    /// <summary>
    /// Ensures that comparing <see cref="SortableItem" />s gives the same results as comparing regular integers.
    /// </summary>
    [TestMethod]
    public void ComparisonsShouldMatchIntegers()
    {
        var array = new ArrayWrapper(starterValues);

        foreach (var (i, j) in StarterIndicesCartesian())
        {
            int intComp = starterValues[i].CompareTo(starterValues[j]);
            int itemComp = array[i].CompareTo(array[j]);
            Assert.AreEqual(intComp, itemComp);
        }
    }

    /// <summary>
    /// Ensures that history is actually pushed to when comparing <see cref="SortableItem">s.
    /// </summary>
    [TestMethod]
    public void ComparisonsShouldBeLogged()
    {
        var array = new ArrayWrapper(starterValues);
        foreach (var (i, j) in StarterIndicesCartesian())
            array[i].CompareTo(array[j]);

        Assert.AreEqual(array.History.Count, starterValues.Length * starterValues.Length);
        Assert.IsTrue(array.History[0] is Compare c1 && c1.ValueA == starterValues[0]);
        Assert.IsTrue(array.History[^1] is Compare c2 && c2.ValueB == starterValues[^1]);
    }
}
