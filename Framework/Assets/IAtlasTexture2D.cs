namespace TrentCOIS.Tools.Visualization.Assets;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents an image that comes from within another larger image.
/// </summary>
public interface IAtlasTexture2D
{
    /// <summary>
    /// The texture that this image gets its data from. Could be a texture atlas/sprite-sheet, or could be a whole
    /// image.
    /// </summary>
    Texture2D SourceTexture { get; }

    /// <summary>
    /// The <c>sourceRect</c> used when drawing this image's <see cref="SourceTexture"/>. Defaults to the entire bounds
    /// of the <see cref="SourceTexture"/>.
    /// </summary>
    Rectangle SourceBounds { get => SourceTexture.Bounds; }
}


/// <summary>
/// Provides extension methods that allow a <see cref="SpriteBatch"/> to draw an <see cref="IAtlasTexture2D"/> (that is,
/// a texture with a source-rectangle built into it).
/// </summary>
public static class SpriteBatchAtlasTextureExtensions
{
    // --- Doc comments copied directly from MonoGame docs. ---
    // https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SpriteBatch.html

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="sb">The current batch.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="destinationRectangle">The drawing bounds on the screen.</param>
    /// <param name="color">A color mask.</param>
    public static void Draw(
        this SpriteBatch sb,
        IAtlasTexture2D texture,
        Rectangle destinationRectangle,
        Color color
    ) => sb.Draw(texture.SourceTexture, destinationRectangle, texture.SourceBounds, color);

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="sb">The current batch.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="destinationRectangle">The drawing bounds on the screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="effects">Modifications for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public static void Draw(
        this SpriteBatch sb,
        IAtlasTexture2D texture,
        Rectangle destinationRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth
    ) => sb.Draw(texture.SourceTexture, destinationRectangle, texture.SourceBounds, color, rotation, origin, effects, layerDepth);

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="sb">The current batch.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    public static void Draw(
        this SpriteBatch sb,
        IAtlasTexture2D texture,
        Vector2 position,
        Color color
    ) => sb.Draw(texture.SourceTexture, position, texture.SourceBounds, color);

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="sb">The current batch.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modifications for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public static void Draw(
        this SpriteBatch sb,
        IAtlasTexture2D texture,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth
    ) => sb.Draw(texture.SourceTexture, position, texture.SourceBounds, color, rotation, origin, scale, effects, layerDepth);

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="sb">The current batch.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modifications for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public static void Draw(
        this SpriteBatch sb,
        IAtlasTexture2D texture,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth
    ) => sb.Draw(texture.SourceTexture, position, texture.SourceBounds, color, rotation, origin, scale, effects, layerDepth);
}
