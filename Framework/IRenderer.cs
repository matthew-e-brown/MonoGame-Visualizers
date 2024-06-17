namespace TrentCOIS.Tools.Visualization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A level of abstraction for performing the actual drawing for a <see cref="Visualization"/>. Implementations of this
/// interface may have their own internal state, so this class has its own <see cref="Update"/> method.
/// </summary>
///
/// <remarks>
/// This interface is separate once again an effort to simplify the code given to students. This way, the rendering code
/// for any given assignment can be tucked away elsewhere.
/// </remarks>
public interface IRenderer
{
    /// <summary>
    /// The graphics device this renderer is expected to use for drawing. Assigned to automatically at game startup.
    /// </summary>
    GraphicsDevice GraphicsDevice { set; }

    /// <summary>
    /// This method is called once the graphics window has been initialized. Any rendering-related setup that couldn't
    /// be done in the constructor should go here. This includes things like the loading of sprites and fonts and the
    /// initialization of any secondary render targets.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// Some objects need to store some data on the graphics card, and so they cannot be initialized until the window
    /// has been setup and a connection to the graphics device has been established.
    /// </para>
    ///
    /// <para>
    /// Note that this method <b>may be called more than once.</b> Specifically, it is called whenever the graphics
    /// device is re-initialized. This can occur when the user adds/removes an external display, when their laptop wakes
    /// up after going to sleep for a while, and so on.
    /// </para>
    /// </remarks>
    public void LoadContent()
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to update any internal state that this renderer may have for itself.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to draw the current state of the attached visualization to the screen.
    /// </summary>
    public void Draw(GameTime gameTime);
}
