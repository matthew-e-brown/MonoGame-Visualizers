namespace COIS2020.Visualization.EntityVisualizer;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Represents any entity that may appear in the visualizer's "arena."
/// </summary>
public class Entity : IComparable<Entity>, IEquatable<Entity>
{
    /// <summary>
    /// The ID that the next-created entity will have. Auto-increments upon each one's creation.
    /// </summary>
    private static ushort nextID = 1;

    /// <summary>
    /// The sprite that gets used when a child class has not re-specified a sprite.
    /// </summary>
    protected readonly static string defaultSprite = "UI/Symbols/QuestionMark";

    // --------------------------------

    /// <summary>
    /// This entity's unique identifier.
    /// </summary>
    public ushort ID { get; private set; }

    /// <summary>
    /// This entity's name, for when it needs to be displayed/written out.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// This entity's <i>(x, y)</i> position in 2D space.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// A string denoting which sprite this entity wants to be drawn with.
    /// </summary>
    ///
    /// <remarks>
    /// The names of the sprites are grouped based on which file from the original asset pack they came from. See
    /// <c>SpriteMap.xml</c> in the <i>Resources</i> folder to determine the group/name combination required for a given
    /// sprite. Not all sprites from the asset pack are included in the map.
    /// </remarks>
    public virtual string SpriteName { get; protected set; }


    // ------------------------------------------------------------------------


    public Entity(string name) : this(name, new Vector2(0, 0))
    { }

    public Entity(string name, float x, float y) : this(name, new Vector2(x, y))
    { }

    public Entity(string name, Vector2 position)
    {
        ID = nextID++;
        Name = name;
        Position = position;
        SpriteName = defaultSprite;
    }


    // ------------------------------------------------------------------------


    /// <summary>
    /// Updates this entity's position.
    /// </summary>
    ///
    /// <param name="dx">An offset to add to this entity's x-position.</param>
    /// <param name="dy">An offset to add to this entity's y-position.</param>
    public void Move(float dx, float dy) => Move(new Vector2(dx, dy));

    /// <summary>
    /// Updates this entity's position.
    /// </summary>
    ///
    /// <param name="deltaPos">An offset vector to add to this entity's position.</param>
    public void Move(Vector2 deltaPos) => Position += deltaPos;

    /// <summary>
    /// Ensures that this entity's position does not fall outside of a given range.
    /// </summary>
    ///
    /// <remarks>
    /// There are two built-in <c>EntityXRange</c> and <c>EntityYRange</c> properties on the base <see
    /// cref="Visualizer"/> class that may be useful here.
    /// </remarks>
    ///
    /// <param name="xRange">A range of allowed x-values for this entity's position (min, max).</param>
    /// <param name="yRange">A range of allowed y-values for this entity's position (min, max).</param>
    public void ClampPosition((float, float) xRange, (float, float) yRange)
    {
        var (xMin, xMax) = xRange;
        var (yMin, yMax) = yRange;
        ClampPosition(xMin, xMax, yMin, yMax);
    }

    /// <summary>
    /// Ensures that this entity's position does not fall outside of a given range.
    /// </summary>
    ///
    /// <remarks>
    /// There are two built-in <c>EntityXRange</c> and <c>EntityYRange</c> properties on the base <see
    /// cref="Visualizer"/> class that may be useful here.
    /// </remarks>
    ///
    /// <param name="xMin">The minimum allowed x-value for this entity's position.</param>
    /// <param name="xMax">The maximum allowed x-value for this entity's position.</param>
    /// <param name="yMin">The minimum allowed y-value for this entity's position.</param>
    /// <param name="yMax">The maximum allowed y-value for this entity's position.</param>
    public void ClampPosition(float xMin, float xMax, float yMin, float yMax)
    {
        float newX = Math.Clamp(Position.X, xMin, xMax);
        float newY = Math.Clamp(Position.Y, yMin, yMax);
        Position = new Vector2(newX, newY);
    }


    // ------------------------------------------------------------------------


    // Just use the name for ToString by default; more involved entities can
    public override string ToString() => Name;

    // Sort by Id; if the other is null, put it after us (MinValue to ensure it's *always* after us).
    public int CompareTo(Entity? other) => (other is null) ? int.MinValue : ID.CompareTo(other.ID);

    // Compare for equality based on ID.
    public bool Equals(Entity? other) => other?.ID.Equals(ID) ?? false;

    // Hash as if this entity was just its ID.
    public override int GetHashCode() => ID.GetHashCode();

    // Throw an error if a non-entity is compared.
    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity) throw new ArgumentException("Object is not a Entity.", nameof(obj));
        return Equals(entity);
    }
}
