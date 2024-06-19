namespace TrentCOIS.Tools.Visualization.Assets;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A loaded texture combined with a <c>sourceRect</c> for drawing a selection from a larger spritesheet/texture atlas.
/// </summary>
/// <seealso cref="IAtlasTexture2D"/>
public class Sprite : IAtlasTexture2D
{
    /// <summary>
    /// This sprite's identifying name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// A reference to the full spritesheet/texture atlas that this sprite's pixel data is taken from.
    /// </summary>
    public Texture2D SourceTexture { get; }

    /// <summary>
    /// The bounds within the underlying <see cref="SourceTexture">spritesheet</see> that this sprite is drawn from.
    /// </summary>
    public Rectangle SourceBounds { get; }


    /// <summary>
    /// Creates a new sprite made of the entire given source texture.
    /// </summary>
    /// <param name="name">The identifier for this sprite.</param>
    /// <param name="sourceTex">The texture that this sprite comes from.</param>
    public Sprite(string name, Texture2D sourceTex) : this(name, sourceTex, new Rectangle(0, 0, sourceTex.Width, sourceTex.Height))
    { }

    /// <summary>
    /// Creates a new sprite made of a specific location on the given source texture.
    /// </summary>
    /// <param name="name">The identifier for this sprite.</param>
    /// <param name="sourceTex"></param>
    /// <param name="sourceBounds"></param>
    public Sprite(string name, Texture2D sourceTex, Rectangle sourceBounds)
    {
        Name = name;
        SourceTexture = sourceTex;
        SourceBounds = sourceBounds;
    }


    /// <summary>
    /// Crops an existing sprite.
    /// </summary>
    /// <param name="sprite">The sprite to crop.</param>
    /// <param name="innerBounds">
    /// Where from within the sprite's bounds to pull the new sprite data, relative to the top-left corner of the
    /// sprite.
    /// </param>
    /// <returns>
    /// A new sprite referencing the same underlying <see cref="SourceTexture"/>, but with different
    /// <see cref="SourceBounds"/>.
    /// </returns>
    public static Sprite Crop(Sprite sprite, Rectangle innerBounds) => Crop(sprite, innerBounds, sprite.Name);


    /// <summary>
    /// Crops an existing sprite and gives the copy a new name.
    /// </summary>
    /// <param name="sprite">The sprite to crop.</param>
    /// <param name="innerBounds">
    /// Where from within the sprite's bounds to pull the new sprite data, relative to the top-left corner of the
    /// sprite.
    /// </param>
    /// <param name="newName">A new identifying name for the cropped sprite.</param>
    /// <returns>
    /// A new sprite referencing the same underlying <see cref="SourceTexture"/>, but with different
    /// <see cref="SourceBounds"/>.
    /// </returns>
    public static Sprite Crop(Sprite sprite, Rectangle innerBounds, string newName)
    {
        Rectangle srcBounds = sprite.SourceBounds;
        innerBounds.Offset(srcBounds.Location);                     // Move TL corner of new bounds
        innerBounds = Rectangle.Intersect(srcBounds, innerBounds);  // Make sure it doesn't overflow the current bounds
        return new Sprite(newName, sprite.SourceTexture, innerBounds);
    }
}
