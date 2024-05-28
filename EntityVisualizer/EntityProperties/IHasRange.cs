namespace TrentCOIS.Tools.Visualization.EntityViz;

using Microsoft.Xna.Framework;

/// <summary>
/// Represents an entity that has some circular "range" around it.
/// </summary>
///
/// <remarks>
/// Any entity that implements this interface will have a circle drawn around it in the main arena.
/// </remarks>
public interface IHasRange
{
    /// <summary>
    /// How large this unit's "range" is, as a radius.
    /// </summary>
    public float Range { get; }

    /// <summary>
    /// What color to draw this unit's circle. Faint red by default.
    /// </summary>
    public Color RangeCircleColor { get => new(0.6f, 0.2f, 0.2f, 0.1f); }
}
