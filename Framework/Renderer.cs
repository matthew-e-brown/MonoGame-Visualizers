namespace TrentCOIS.Tools.Visualization;

using System;
using Microsoft.Xna.Framework;


/// <summary>
/// A level of abstraction for performing the actual drawing for a <see cref="Visualization"/>. Implementations of this
/// interface may have their own internal state, so this class has its own <see cref="Update"/> method.
/// </summary>
///
/// <remarks>
/// This interface is separate once again an effort to simplify the code given to students. This way, the rendering code
/// for any given assignment can be tucked away elsewhere.
/// </remarks>
public abstract class Renderer<V> where V : Visualization
{
    private GameRunner<V>? currentRunner; // Used to get access to internal methods while running

    private int windowWidth = 1280;
    private int windowHeight = 720;

    /// <summary>How wide the game window should be. Must be set before running the visualization.</summary>
    /// <remarks>The default value is 1280 pixels.</remarks>
    public int WindowWidth { get => windowWidth; init => windowWidth = value; }

    /// <summary>How tall the game window should be. Must be set before running the visualization.</summary>
    /// <remarks>The default value is 720 pixels.</remarks>
    public int WindowHeight { get => windowHeight; init => windowHeight = value; }


    /// <summary>
    /// How long to wait between updating the user's visualization.
    /// </summary>
    public TimeSpan FrameDelay { get; set; }

    /// <summary>
    /// <see cref="FrameDelay"/>, but in milliseconds.
    /// </summary>
    public long FrameDelayMS
    {
        get => FrameDelay.Milliseconds;
        set => FrameDelay = new TimeSpan(value * TimeSpan.TicksPerMillisecond);
    }


    /// <summary>
    /// Sets a reference to the graphics device manager.
    /// </summary>
    /// <remarks>
    /// Despite not being declared as nullable, this property <b>will be <c>null</c></b> until after
    /// <see cref="Run(V)"/> is called. It is guaranteed to be non-null in all of the virtual methods of this class.
    /// </remarks>
    protected internal GraphicsDeviceManager Graphics { get; internal set; }


    /// <summary>
    /// Creates a new <see cref="Renderer{V}"/> instance.
    /// </summary>
    public Renderer()
    {
        Graphics = null!; // This is set by the
    }


    /// <summary>
    /// Sets <see cref="WindowWidth"/> and <see cref="WindowHeight"/> at the same time. This is the only way to change
    /// the window size after the visualization has begun.
    /// </summary>
    /// <param name="newWidth"></param>
    /// <param name="newHeight"></param>
    protected void ResizeWindow(int newWidth, int newHeight)
    {
        windowWidth = newWidth;
        windowHeight = newHeight;
        currentRunner?.ResizeWindow(newWidth, newHeight);
    }


    /// <summary>
    /// Sets up everything this renderer needs to render the user's visualization.
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    public virtual void Initialize(V userViz)
    {
        // Default implementation does nothing.
    }

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
    /// <param name="userViz">A reference to the user's visualization.</param>
    public virtual void LoadContent(V userViz)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to update any internal state that this renderer may have for itself.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="userViz">A reference to the user's visualization.</param>
    public virtual void Update(GameTime gameTime, V userViz)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to draw the current state of the attached visualization to the screen.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="userViz">A reference to the user's visualization.</param>
    public abstract void Draw(GameTime gameTime, V userViz);


    /// <summary>Use this renderer to run the given visualization.</summary>
    /// <param name="visualization">The visualization to run.</param>
    public void Run(V visualization) => Run(visualization, true);

    /// <summary>Use this renderer to run the given visualization.</summary>
    /// <param name="visualization">The visualization to run.</param>
    /// <param name="startPaused">Whether or not this visualization should start right away or not.</param>
    public void Run(V visualization, bool startPaused)
    {
        using var runner = new GameRunner<V>(visualization, this)
        {
            IsPaused = startPaused
        };

        currentRunner = runner; // Keep reference so we can access methods.
        runner.Run();           // Run the game
        currentRunner = null;   // Ensure dispose runs properly (I don't trust it if we keep a reference around)
    }
}
