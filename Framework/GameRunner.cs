namespace TrentCOIS.Tools.Visualization;

using System;
using Microsoft.Xna.Framework;


/// <summary>
/// Configuration for the execution of a <see cref="Visualization"/>.
/// </summary>
public record RunOptions
{
    /// <summary>How wide of a window to create.</summary>
    /// <remarks>The default value is 1280 pixels.</remarks>
    public int WindowWidth = 1280;

    /// <summary>How tall of a window to create.</summary>
    /// <remarks>The default value is 720 pixels.</remarks>
    public int WindowHeight = 720;

    /// <summary>Controls whether or not the visualization starts right away.</summary>
    /// <remarks>The default is to start paused.</remarks>
    public bool StartPaused = true;

    /// <summary>The amount of delay between each step of a <see cref="Visualization"/>.</summary>
    /// <remarks>The default value is 100 ms.</remarks>
    public long FrameDelayMS = 100;

    /// <summary>
    /// The default rendering options.
    /// </summary>
    public readonly static RunOptions DefaultOptions = new(); // uses default member values.
}


/// <summary>
/// Handles communicating with the underlying <see cref="Game">the MonoGame <c>Game</c> class</see>.
/// </summary>
///
/// <remarks>
/// The extra level of encapsulation between <see cref="Visualization"/> and <see cref="GameRunner"/> is so that the
/// students never have direct access to the members <see cref="Game"/> that MonoGame makes public by default.
/// </remarks>
public class GameRunner : Game
{
    /// <summary>
    /// A reference to the user's visualization.
    /// </summary>
    protected Visualization Parent { get; private init; }

    /// <summary>
    /// The <see cref="GraphicsDeviceManager" /> for this <see cref="Game">MonoGame <c>Game</c></see> instance.
    /// </summary>
    protected GraphicsDeviceManager Graphics { get; private set; }


    /// <summary>Gets or initializes the window's width.</summary>
    /// <value>The width of the window, in pixels.</value>
    /// <seealso cref="WindowHeight"/>
    public int WindowWidth { get; private set; }

    /// <summary>Gets or initializes the window's height.</summary>
    /// <value>The height of the window, in pixels.</value>
    /// <seealso cref="WindowWidth"/>
    public int WindowHeight { get; private set; }

    /// <summary>
    /// Gets or sets a value that determines the playback-state of the visualization.
    /// </summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPlaying { get; set; }

    /// <summary>
    /// Gets or sets a value that determines the playback-state of the visualization.
    /// </summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPaused { get => !IsPlaying; set => IsPlaying = !value; }

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
    /// The current game frame, or the number of "ticks" that have elapsed since the start of the simulation.
    /// </summary>
    ///
    /// <remarks>
    /// For all intents and purposes, this counter is 1-based. It starts at zero, but it is incremented before the first
    /// time the user's <see cref="Visualization.Update"/> method is called. That way, tick number "zero" is the one
    /// seen before any stepping occurs.
    /// </remarks>
    public uint CurrentFrame { get; protected set; } = 0;

    /// <summary>
    /// The most recent time that <see cref="Visualization.Update"/> was called.
    /// </summary>
    protected TimeSpan LastTickedTime { get; private set; }

    // Used by methods to check/set the LastTickedTime even if they don't get it as a parameter.
    private TimeSpan? currentTime;


    /// <summary>
    /// Creates a new renderer that will run the provided visualization.
    /// </summary>
    /// <param name="visualization">The visualization to render.</param>
    public GameRunner(Visualization visualization)
    {
        Parent = visualization;
        Parent.Runner = this;

        Graphics = new GraphicsDeviceManager(this);
        Components.Add(visualization.UserInput.ComponentInstance);
    }


    /// <summary>
    /// Creates and configures a new renderer that will run the provided visualization.
    /// </summary>
    /// <param name="visualization">The visualization to render.</param>
    /// <param name="options">Configuration for how this renderer should behave.</param>
    public GameRunner(Visualization visualization, RunOptions? options) : this(visualization)
    {
        options ??= RunOptions.DefaultOptions;
        WindowWidth = options.WindowWidth;
        WindowHeight = options.WindowHeight;
        IsPaused = options.StartPaused;
        FrameDelayMS = options.FrameDelayMS;
    }


    /// <summary>
    /// Runs at start-up once the graphics device has been set up.
    /// </summary>
    protected override void Initialize()
    {
        // Configure the main window
        Window.AllowUserResizing = false;
        Graphics.IsFullScreen = false;
        Graphics.PreferredBackBufferWidth = WindowWidth;
        Graphics.PreferredBackBufferHeight = WindowHeight;
        Graphics.ApplyChanges();

        Parent.Renderer.GraphicsDevice = GraphicsDevice;

        // Unlike Load/Update/Draw, Initialization of our components comes *after* we setup our stuff (since it's
        // important).
        base.Initialize();
    }


    /// <summary>
    /// Runs at startup and whenever a graphics device reset occurs.
    /// </summary>
    protected override void LoadContent()
    {
        base.LoadContent();
        Parent.Renderer.LoadContent();
    }


    /// <summary>
    /// Runs on every frame to handle game updates.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Update(GameTime gameTime)
    {
        currentTime = gameTime.TotalGameTime;

        // Call the user's Input method after we update our components (i.e. the InputManager, since they need that).
        base.Update(gameTime);
        Parent.HandleInput(gameTime);

        // Call user Update method
        if (IsPlaying && LastTickedTime + FrameDelay <= currentTime)
        {
            CurrentFrame++;
            DoUserUpdate();
        }

        // Once the user's visualization is updated, update the renderer's state for drawing.
        Parent.Renderer.Update(gameTime);
        currentTime = null;
    }


    private void DoUserUpdate()
    {
        if (currentTime is not TimeSpan time)
            throw new InvalidOperationException("Called DoUserUpdate from outside of frame update.");
        Parent.Update(CurrentFrame);
        LastTickedTime = time;
    }

    internal void SingleStepForward()
    {
        CurrentFrame++;
        DoUserUpdate();
    }

    internal void SingleStepBackward()
    {
        CurrentFrame--;
        DoUserUpdate();
    }



    /// <summary>
    /// Runs on every frame to handle rendering.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Draw(GameTime gameTime)
    {
        Graphics.GraphicsDevice.Clear(Color.Black);

        // The user's `Draw` method is called every single frame, even if their `Update` method didn't get called. That
        // allows their `HandleInput` method to update animation state.
        base.Draw(gameTime);            // Components (if any) get updated first.
        Parent.Renderer.Draw(gameTime); // Then the parent renderer does its stuff.
    }
}
