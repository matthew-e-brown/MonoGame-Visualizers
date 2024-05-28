namespace TrentCOIS.Tools.Visualization.EntityViz;

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

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    public Entity(string name) : this(name, new Vector2(0, 0))
    { }

    /// <summary>
    /// Creates a new entity at the given position.
    /// </summary>
    public Entity(string name, float x, float y) : this(name, new Vector2(x, y))
    { }

    /// <summary>
    /// Creates a new entity at the given position.
    /// </summary>
    public Entity(string name, Vector2 position)
    {
        ID = nextID++;
        Name = name;
        Position = position;
        SpriteName = defaultSprite;
    }


    /// <summary>Converts this entity into a string.</summary>
    /// <remarks>By default, this just returns the entity's name.</remarks>
    public override string ToString() => Name;

    /// <summary>Compares this entity against another.</summary>
    /// <remarks>Comparisons are made using the entities' IDs. A null value is sorted after all others.</remarks>
    public int CompareTo(Entity? other) => (other is null) ? int.MinValue : ID.CompareTo(other.ID);

    /// <summary>Compares for entity equality based on ID.</summary>
    public bool Equals(Entity? other) => other?.ID.Equals(ID) ?? false;

    /// <summary>Gets the hash code for this entity.</summary>
    /// <remarks>This simply returns the <see cref="uint.GetHashCode">hash code of this entity's ID.</see></remarks>
    public override int GetHashCode() => ID.GetHashCode();

    /// <summary>Catch-all version of <see cref="Equals(Entity?)"/>.</summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity) throw new ArgumentException("Object is not a Entity.", nameof(obj));
        return Equals(entity);
    }
}
