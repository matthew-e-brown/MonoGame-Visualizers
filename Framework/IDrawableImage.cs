namespace TrentCOIS.Tools.Visualization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents something that can be drawn onto the screen. This interface provides convenience methods for drawing
/// </summary>
public interface IDrawableImage
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

    /// <summary>
    /// How wide this image should be drawn, in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// How tall this image should be drawn, in pixels.
    /// </summary>
    int Height { get; }
}
