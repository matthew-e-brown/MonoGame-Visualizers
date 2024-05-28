namespace TrentCOIS.Tools.Visualization.EntityViz;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// A wrapper object that contains both a <see cref="Rectangle">region to be drawn within</see> as well as the
/// <see cref="RenderTarget2D">render target it gets rendered onto.</see>
/// </summary>
internal class RenderPanel
{
    public delegate void DrawMethod(SpriteBatch spriteBatch, GameTime gameTime);

    protected Rectangle region;
    protected RenderTarget2D target;

    /// <summary>
    /// The texture that this panel has been rendered onto.
    /// </summary>
    public Texture2D Texture => target;

    /// <summary>
    /// Where on the screen this panel is to be drawn.
    /// </summary>
    public Rectangle Destination => region;

    protected SpriteBatch SpriteBatch { get; init; }
    protected GraphicsDevice GraphicsDevice { get; init; }

    protected bool forceDirty = true;
    protected bool userDirty = false;

    /// <summary>
    /// How to draw onto this surface. Only executed if the panel is not currently <see cref="Dirty">dirty</see>.
    /// </summary>
    public DrawMethod? DrawFn { get; set; }

    /// <summary>
    /// Whether or not the panel needs to be re-drawn on the next frame. Reset automatically when a redraw occurs.
    /// </summary>
    public bool Dirty
    {
        get => forceDirty || userDirty;
        set => userDirty = value;
    }

    public int X { get => region.X; set => region.X = value; }
    public int Y { get => region.Y; set => region.Y = value; }

    public int Width
    {
        get => region.Width;
        set
        {
            region.Width = value;
            target = InitRenderTarget();
            forceDirty = true;
        }
    }

    public int Height
    {
        get => region.Height;
        set
        {
            region.Height = value;
            target = InitRenderTarget();
            forceDirty = true;
        }
    }


    public RenderPanel(GraphicsDevice graphicsDevice, int x, int y, int width, int height)
    {
        GraphicsDevice = graphicsDevice;
        SpriteBatch = new SpriteBatch(graphicsDevice);

        region = new(x, y, width, height);
        target = InitRenderTarget();

        forceDirty = true;
    }


    protected RenderTarget2D InitRenderTarget()
    {
        return new RenderTarget2D(
            GraphicsDevice,
            region.Width,
            region.Height,
            mipMap: false,
            SurfaceFormat.Color,
            DepthFormat.Depth24,
            preferredMultiSampleCount: 4,
            RenderTargetUsage.PreserveContents
        );
    }


    /// <summary>
    /// Executes this panel's draw method if the panel is dirty.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    /// <returns>True if the panel was dirty and a redraw took place.</returns>
    public bool RedrawContents(GameTime gameTime)
    {
        if (!Dirty || DrawFn is null)
            return false;

        GraphicsDevice.SetRenderTarget(target);     // Draw to texture
        GraphicsDevice.Clear(Color.Transparent);    // Wipe texture to transparent
        DrawFn.Invoke(SpriteBatch, gameTime);       // Call user's drawing function
        GraphicsDevice.SetRenderTarget(null);       // Resume drawing to main canvas

        userDirty = forceDirty = false;
        return true;
    }


    /// <summary>
    /// Checks whether or not the mouse is currently over this panel.
    /// </summary>
    /// <param name="mouse">The current mouse state.</param>
    /// <returns>True if the mouse is over top of this panel, false otherwise.</returns>
    public bool IsMouseOver(MouseState mouse)
    {
        (int l, int r) = (region.X, region.X + region.Width);
        (int t, int b) = (region.Y, region.Y + region.Height);
        bool inBoundsH = l <= mouse.X && mouse.X <= r;
        bool inBoundsV = t <= mouse.Y && mouse.Y <= b;
        return inBoundsH && inBoundsV;
    }
}
