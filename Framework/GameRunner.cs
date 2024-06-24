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
internal class GameRunner<V> : Game where V : Visualization
{
    /// <summary>
    /// A reference to the user's visualization.
    /// </summary>
    public V UserViz { get; private init; }

    /// <summary>
    /// The renderer used to render <see cref="UserViz"/>.
    /// </summary>
    public Renderer<V> Renderer { get; set; }

    /// <summary>
    /// The <see cref="GraphicsDeviceManager" /> for this <see cref="Game">MonoGame <c>Game</c></see> instance.
    /// </summary>
    protected GraphicsDeviceManager Graphics { get; private set; }


    /// <summary>
    /// The current game frame, or the number of "ticks" that have elapsed since the start of the simulation.
    /// </summary>
    ///
    /// <remarks>
    /// For all intents and purposes, this counter is 1-based. It starts at zero, but it is incremented before the first
    /// time the user's <see cref="Visualization.Update"/> method is called. That way, tick number "zero" is the one
    /// seen before any stepping occurs.
    /// </remarks>
    public uint CurrentFrame { get; internal set; } = 0;

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
        Graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;

        UserViz = visualization;
        Renderer = renderer;
        Renderer.Graphics = Graphics;
    }


    /// <summary>
    /// Runs at start-up once the graphics device has been set up.
    /// </summary>
    protected override void Initialize()
    {
        // Configure the main window
        Window.AllowUserResizing = false;
        Graphics.IsFullScreen = false;
        ResizeWindow(Renderer.WindowWidth, Renderer.WindowHeight);

        // Components initialization comes *after* we setup our window (since that's an important step).
        base.Initialize();
        Renderer.Initialize(UserViz, UserViz.UserInput);
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
        Renderer.LoadContent(UserViz);
    }


    #region Update

    /// <summary>
    /// Runs on every frame to handle game updates.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Update(GameTime gameTime)
    {
        currentTime = gameTime.TotalGameTime;

        base.Update(gameTime); // Update components (if any)

        // Check if we need to run the user's update method during this frame
        bool doUserUpdate = Renderer.IsPlaying && LastTickedTime + Renderer.FrameDelay <= currentTime;
        if (doUserUpdate) CurrentFrame++;

        // Run input handling and updates, starting with renderer's input and PreUpdate phases
        Renderer.UserInput.Update(gameTime);
        Renderer.HandleInput(UserViz, gameTime, UserViz.UserInput);
        Renderer.PreUpdate(UserViz, gameTime, CurrentFrame, doUserUpdate);

        // Then the user's update
        UserViz.UserInput.Update(gameTime);
        UserViz.HandleInput(gameTime.TotalGameTime);
        if (doUserUpdate) DoUserUpdate();

        // Followed by our PostUpdate pass.
        Renderer.PostUpdate(UserViz, gameTime, CurrentFrame, doUserUpdate); // Our post-update pass

        currentTime = null;
    }


    internal void DoUserUpdate()
    {
        if (currentTime is not TimeSpan time)
            throw new InvalidOperationException("Called DoUserUpdate from outside of frame update.");
        UserViz.Update(CurrentFrame);
        LastTickedTime = time;
    }

    #endregion


    #region Draw

    /// <summary>
    /// Runs on every frame to handle rendering.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);                // Draw components (if any).
        Renderer.Draw(UserViz, gameTime);   // Don't clear the screen: let the renderer handle that.
    }

    #endregion
}
