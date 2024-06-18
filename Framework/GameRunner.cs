namespace TrentCOIS.Tools.Visualization;

using System;
using Microsoft.Xna.Framework;


/// <summary>
/// Handles communicating with the underlying <see cref="Game">the MonoGame <c>Game</c> class</see>.
/// </summary>
///
/// <remarks>
/// The extra level of encapsulation between <see cref="Visualization"/> and <see cref="GameRunner{V}"/> is so that the
/// students never have direct access to the members <see cref="Game"/> that MonoGame makes public by default.
/// </remarks>
public class GameRunner<V> : Game where V : Visualization
{
    /// <summary>
    /// A reference to the user's visualization.
    /// </summary>
    public V UserViz { get; private init; }

    /// <summary>
    /// The renderer used to render <see cref="UserViz"/>.
    /// </summary>
    public Renderer<V> VizRenderer { get; set; }

    /// <summary>
    /// The <see cref="GraphicsDeviceManager" /> for this <see cref="Game">MonoGame <c>Game</c></see> instance.
    /// </summary>
    protected GraphicsDeviceManager Graphics { get; private set; }


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
    /// <param name="renderer">
    /// The renderer implementation to call on to draw the user's visualization to the screen.
    /// </param>
    public GameRunner(V visualization, Renderer<V> renderer)
    {
        UserViz = visualization;
        VizRenderer = renderer;

        Graphics = new GraphicsDeviceManager(this);
        VizRenderer.Graphics = Graphics;

        UserViz.UserPause += HandleUserPause;
        UserViz.UserPause += HandleUserResume;
        UserViz.UserStepForward += HandleUserStepForward;
        UserViz.UserStepBackward += HandleUserStepBackward;
        UserViz.UserExit += HandleUserExit;
    }


    /// <summary>
    /// Runs at start-up once the graphics device has been set up.
    /// </summary>
    protected override void Initialize()
    {
        // Configure the main window
        Window.AllowUserResizing = false;
        Graphics.IsFullScreen = false;
        ResizeWindow(VizRenderer.WindowWidth, VizRenderer.WindowHeight);

        // Unlike Load/Update/Draw, Initialization of our components comes *after* we setup our stuff (since it's
        // important).
        base.Initialize();
        VizRenderer.Initialize(UserViz);

        // Start with a dark gray screen.
        Graphics.GraphicsDevice.Clear(new Color(14, 14, 14));
    }


    /// <summary>
    /// Resizes the main window
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected internal void ResizeWindow(int width, int height)
    {
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.ApplyChanges();
    }


    /// <summary>
    /// Runs at startup and whenever a graphics device reset occurs.
    /// </summary>
    protected override void LoadContent()
    {
        base.LoadContent();
        VizRenderer.LoadContent(UserViz);
    }


    #region Update

    /// <summary>
    /// Runs on every frame to handle game updates.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Update(GameTime gameTime)
    {
        currentTime = gameTime.TotalGameTime;

        // Call the user's Input method after we update our components (i.e. the InputManager, since they need that).
        base.Update(gameTime);
        UserViz.HandleInput(gameTime.TotalGameTime);

        // Call user Update method
        if (IsPlaying && LastTickedTime + VizRenderer.FrameDelay <= currentTime)
        {
            CurrentFrame++;
            DoUserUpdate();
        }

        // Once the user's visualization is updated, update the renderer's state so it can draw it.
        VizRenderer.Update(gameTime, UserViz);
        currentTime = null;
    }


    private void DoUserUpdate()
    {
        if (currentTime is not TimeSpan time)
            throw new InvalidOperationException("Called DoUserUpdate from outside of frame update.");
        UserViz.Update(CurrentFrame);
        LastTickedTime = time;
    }


    /// <summary>This method is run when the <see cref="Visualization.UserPause"/> event is fired.</summary>
    protected virtual void HandleUserPause() => IsPaused = true;

    /// <summary>This method is run when the <see cref="Visualization.UserResume"/> event is fired.</summary>
    protected virtual void HandleUserResume() => IsPlaying = true;

    /// <summary>This method is run when the <see cref="Visualization.UserStepForward"/> event is fired.</summary>
    protected virtual void HandleUserStepForward()
    {
        if (IsPaused)
        {
            CurrentFrame++;
            DoUserUpdate();
        }
    }

    /// <summary>This method is run when the <see cref="Visualization.UserStepBackward"/> event is fired.</summary>
    protected virtual void HandleUserStepBackward()
    {
        if (IsPaused)
        {
            CurrentFrame--;
            DoUserUpdate();
        }
    }

    /// <summary>This method is run when the <see cref="Visualization.UserExit"/> event is fired.</summary>
    protected virtual void HandleUserExit() => Exit();

    #endregion


    #region Draw

    /// <summary>
    /// Runs on every frame to handle rendering.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Draw(GameTime gameTime)
    {
        // Don't clear the screen: we let them handle that.
        base.Draw(gameTime);                    // Components (if any) get updated first.
        VizRenderer.Draw(gameTime, UserViz);    // Then the parent renderer does its stuff.
    }

    #endregion
}
