namespace TrentCOIS.Tools.Visualization;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


/// <summary>
/// Configuration for the rendering of a <see cref="Visualization"/>.
/// </summary>
public record RenderOptions
{
    /// <summary>How wide of a window to create. The default value is 1280 pixels.</summary>
    public int WindowWidth;
    /// <summary>How tall of a window to create. The default value is 720 pixels</summary>
    public int WindowHeight;
    /// <summary>Controls whether or not the visualization starts right away. The default is to start paused.</summary>
    public bool StartPaused;

    /// <summary>
    /// The default rendering options.
    /// </summary>
    public readonly static RenderOptions DefaultOptions = new()
    {
        WindowWidth = 1280,
        WindowHeight = 720,
        StartPaused = true,
    };
}


/// <summary>
/// This class is what actually implements all the functionality of <see cref="Game">the MonoGame <c>Game</c>
/// class</see>.
/// </summary>
///
/// <remarks>
/// The extra level of encapsulation between <see cref="Visualization"/> and <see cref="Renderer"/> is so that the
/// students never have direct access to the members <see cref="Game"/> that MonoGame makes public by default.
/// </remarks>
public class Renderer : Game
{
    /// <summary>
    /// A reference to the user's visualization.
    /// </summary>
    protected Visualization UserViz { get; private init; }

    /// <summary>
    /// The <see cref="GraphicsDeviceManager" /> for this <see cref="Game">MonoGame <c>Game</c></see> instance.
    /// </summary>
    protected GraphicsDeviceManager Graphics { get; private set; }


    /// <summary>
    /// The main <see cref="SpriteBatch"/> used to draw to sprites the screen.
    /// </summary>
    /// <remarks>
    /// This property is null until the graphics device has been initialized and <see cref="LoadContent"/> has been run.
    /// Use the <see cref="GraphicsResetComplete"/> property to check if this is the case.
    /// </remarks>
    protected SpriteBatch? MainSpriteBatch { get; private set; }

    /// <summary>
    /// The surface that is given to the user's <see cref="Visualization.Draw" /> method. After their method is called,
    /// </summary>
    /// <remarks>
    /// This property is null until the graphics device has been initialized and <see cref="LoadContent"/> has been run.
    /// Use the <see cref="GraphicsResetComplete"/> property to check if this is the case.
    /// </remarks>
    protected Surface? MainScreen { get; private set; }


    /// <summary>
    /// Whether or not the graphics device has been initialized and <see cref="LoadContent"/> has been run.
    /// </summary>
    /// <remarks>
    /// This variable is updated by the <see cref="GraphicsDeviceManager.DeviceResetting"/> and the
    /// <see cref="GraphicsDeviceManager.DeviceReset"/> events.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(MainSpriteBatch), nameof(MainScreen))]
    protected bool GraphicsResetComplete { get; private set; }


    /// <summary>Gets or initializes the window's width.</summary>
    /// <value>The width of the window, in pixels.</value>
    /// <seealso cref="WindowHeight"/>
    public int WindowWidth { get; private init; }

    /// <summary>Gets or initializes the window's height.</summary>
    /// <value>The height of the window, in pixels.</value>
    /// <seealso cref="WindowWidth"/>
    public int WindowHeight { get; private init; }


    /// <summary>
    /// Gets or sets a value that determines the playback-state of the visualization.
    /// </summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPlaying { get; set; }

    /// <summary>
    /// Gets or sets a value that determines the playback-state of the visualization.
    /// </summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPaused
    {
        get => !IsPlaying;
        set => IsPlaying = !value;
    }


    /// <summary>
    /// Creates a new renderer that will run the provided visualization.
    /// </summary>
    /// <param name="visualization">The visualization to render.</param>
    public Renderer(Visualization visualization)
    {
        UserViz = visualization;
        Graphics = new GraphicsDeviceManager(this);
    }


    /// <summary>
    /// Creates and configures a new renderer that will run the provided visualization.
    /// </summary>
    /// <param name="visualization">The visualization to render.</param>
    /// <param name="options">Configuration for how this renderer should behave.</param>
    public Renderer(Visualization visualization, RenderOptions? options) : this(visualization)
    {
        options ??= RenderOptions.DefaultOptions;

        WindowWidth = options.WindowWidth;
        WindowHeight = options.WindowHeight;
        IsPaused = options.StartPaused;
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

        Graphics.DeviceResetting += OnGraphicsResetting;
        Graphics.DeviceReset += OnGraphicsReset;

        Graphics.ApplyChanges();

        // Unlike Load/Update/Draw, components' Initialization comes after we setup our stuff.
        base.Initialize();
    }

    private void OnGraphicsResetting(object? sender, EventArgs args) => GraphicsResetComplete = false;
    private void OnGraphicsReset(object? sender, EventArgs args) => GraphicsResetComplete = true;


    /// <summary>
    /// Runs at startup and whenever a graphics device reset occurs.
    /// </summary>
    protected override void LoadContent()
    {
        MainSpriteBatch = new SpriteBatch(GraphicsDevice);
        MainScreen = new Surface();

        base.LoadContent();     // Components' content (if any).
        UserViz.LoadContent();  // The user's content.
    }


    /// <summary>
    /// Runs on every frame to handle game updates.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime); // Components (if any) get updated first.
        UserViz.HandleInput();
    }


    /// <summary>
    /// Runs on every frame to handle rendering.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected override void Draw(GameTime gameTime)
    {
        if (!GraphicsResetComplete)
        {
            // [FIXME] Should be System.Diagnostics.UnreachableException, but that's not available until net7.0. Once we
            // can update, this should be switched.
            throw new InvalidOperationException("Entered Draw method before graphics were initialized.");
        }

        Graphics.GraphicsDevice.Clear(Color.Black);

        // The user's `Draw` method is called every single frame, even if their `Update` method didn't get called. That
        // allows their `HandleInput` method to update animation state.
        base.Draw(gameTime);        // Components (if any) get drawn first.
        UserViz.Draw(MainScreen);   // Then the visualization.
    }
}
