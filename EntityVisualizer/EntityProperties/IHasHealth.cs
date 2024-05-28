namespace COIS2020.Visualization.EntityVisualizer;

/// <summary>
/// Represents an entity that has HP (for "hit points," AKA health).
/// </summary>
///
/// <remarks>
/// Any entity that implements this interface will have a health-bar drawn in the right-hand panel in the <see
/// cref="Visualizer"/>.
/// </remarks>
public interface IHasHP
{
    /// <summary>
    /// How much more damage this entity can currently withstand.
    /// </summary>
    public int HP { get; }

    /// <summary>
    /// How much damage this entity can withstand in total.
    /// </summary>
    public int MaxHP { get; }
}
