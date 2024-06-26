namespace TrentCOIS.Tools.Visualization;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using TrentCOIS.Tools.Visualization.Input;

/// <summary>
/// The base class for all visualization renderers. The student's code goes into a <see cref="Visualization"/>, which a
/// simpler set of methods/properties; provided assignment code goes into a <see cref="Renderer{V}"/>, where
/// <typeparamref name="V"/> is the name of the student's implementation of <see cref="Visualization"/>.
/// </summary>
///
/// <typeparam name="V">The class of visualization which this renderer knows how to draw to the screen.</typeparam>
///
/// <remarks>
/// <para>
/// The full lifecycle of each frame is as follows.
///
/// <list type="number">
///     <item>The renderer's <see cref="UserInput"/> is updated.</item>
///     <item>The renderer's <see cref="HandleInput"/> method is called.</item>
///     <item>The renderer's <see cref="PreUpdate"/> method is called.</item>
///     <item>The user's <see cref="Visualization.UserInput"/> is updated.</item>
///     <item>The user's <see cref="HandleInput"/> method is called.</item>
///     <item>
///         If applicable (see <see cref="FrameDelay"/>), the user's <see cref="Visualization.Update"/> method is
///         called.
///     </item>
///     <item>The renderer's <see cref="PostUpdate"/> method is called.</item>
/// </list>
///
/// After the above has completed, <see cref="Draw"/> is called. The user's <see cref="Visualization"/> has no
/// <c>Draw</c> method.
/// </para>
///
/// <para>
/// Also note that the user has no <c>Initialize</c> method; their "game" initialization is expected to be done in the
/// constructor.
/// </para>
/// </remarks>
public abstract class Renderer<V> where V : Visualization
{
    private GameRunner<V>? currentRunner; // Used to get access to internal methods while running

    private int windowWidth = 1280;
    private int windowHeight = 720;

    /// <summary>
    /// How wide the game window should be when it opens.
    /// </summary>
    /// <remarks>The default value is 1280 pixels.</remarks>
    /// <seealso cref="Renderer(int, int)">To create a renderer with a given window size.</seealso>
    /// <seealso cref="ResizeWindow">To change the size of the window.</seealso>
    public int WindowWidth { get => windowWidth; }

    /// <summary>
    /// How tall the game window should be when it opens.
    /// </summary>
    /// <remarks>The default value is 720 pixels.</remarks>
    /// <seealso cref="Renderer(int, int)">To create a renderer with a given window size.</seealso>
    /// <seealso cref="ResizeWindow">To change the size of the window.</seealso>
    public int WindowHeight { get => windowHeight; }


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
    /// <remarks>
    /// For most purposes, this counter is essentially 1-based. It starts at zero, but it is incremented before the
    /// first time the user's <see cref="Visualization.Update"/> method is called. That way, tick number "zero" is the
    /// one seen before any stepping occurs (or when stepping backwards into frame #0).
    /// </remarks>
    public uint CurrentFrame { get; internal set; } = 0;

    /// <summary>Whether or not the visualization is currently playing.</summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPlaying { get; set; }

    /// <summary>Whether or not the visualization is currently paused.</summary>
    /// <seealso cref="IsPlaying"/>
    public bool IsPaused { get => !IsPlaying; set => IsPlaying = !value; }

    /// <summary>
    /// This renderer's own <see cref="InputManager"/> onto which it may attach events. This <see cref="InputManager"/>
    /// is updated before the user's.
    /// </summary>
    /// <seealso cref="PreUpdate"/>
    protected internal InputManager UserInput { get; }

    /// <summary>
    /// Gets or sets a reference to the graphics device manager.
    /// </summary>
    /// <remarks>
    /// Despite not being declared as nullable, this property <b>will be <c>null</c> until after <see cref="Run(V)"/> is
    /// called.</b> It is set internally once MonoGame has started, and is guaranteed to be non-null in all of the other
    /// lifecycle methods of this class.
    /// </remarks>
    protected internal GraphicsDeviceManager Graphics { get; internal set; }


    /// <summary>
    /// Creates a new <see cref="Renderer{V}"/> instance.
    /// </summary>
    public Renderer()
    {
        Graphics = null!; // This is set by the internal renderer before initialization.
        IsPaused = true;
        UserInput = new InputManager();
    }

    /// <summary>
    /// Creates a new <see cref="Renderer{V}"/> instance.
    /// </summary>
    /// <param name="windowWidth">How wide to make the window.</param>
    /// <param name="windowHeight">How tall to make the window.</param>
    /// <seealso cref="ResizeWindow">To change the size of the window after initialization.</seealso>
    public Renderer(int windowWidth, int windowHeight) : this()
    {
        this.windowWidth = windowWidth;
        this.windowHeight = windowHeight;
    }


    /// <summary>
    /// Sets <see cref="WindowWidth"/> and <see cref="WindowHeight"/> at the same time. This is the only way to change
    /// the window size after the visualization has begun.
    /// </summary>
    /// <param name="newWidth">The new width of the game window.</param>
    /// <param name="newHeight">The new height of the game window.</param>
    protected void ResizeWindow(int newWidth, int newHeight)
    {
        windowWidth = newWidth;
        windowHeight = newHeight;
        currentRunner?.ResizeWindow(newWidth, newHeight);
    }


    #region Run method

    /// <summary>Use this renderer to run the given visualization.</summary>
    /// <param name="visualization">The visualization to run.</param>
    public void Run(V visualization)
    {
        using var runner = new GameRunner<V>(visualization, this);

        visualization.UserPause += HandleUserPause;
        visualization.UserResume += HandleUserResume;
        visualization.UserStepForward += HandleUserStepForward;
        visualization.UserStepBackward += HandleUserStepBackward;
        visualization.UserExit += HandleUserExit;

        try
        {
            currentRunner = runner; // Keep reference so we can access methods.
            runner.Run();           // Run the game ("blocks" until Exit is called). GameRunner will call our methods.
            currentRunner = null;   // Release system resources (runner is using'd)
        }
        finally
        {
            Cleanup();
        }
    }


    /// <summary>Use this renderer to run the given visualization.</summary>
    /// <param name="visualization">The visualization to run.</param>
    /// <param name="startPaused">
    /// Defaults to <c>true</c>. Set to <c>false</c> to have the visualization start right away.
    /// </param>
    public void Run(V visualization, bool startPaused)
    {
        IsPaused = startPaused;
        Run(visualization);
    }

    #endregion


    #region Virtual methods

    /// <summary>
    /// Sets up everything this renderer needs to render the user's visualization.
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <remarks>
    /// The base implementation of this method does nothing.
    /// </remarks>
    protected internal virtual void Initialize(V userViz)
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
    ///
    /// <para>
    /// The base implementation of this method does nothing.
    /// </para>
    /// </remarks>
    /// <param name="userViz">A reference to the user's visualization.</param>
    protected internal virtual void LoadContent(V userViz)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame, before the user's <see cref="Visualization.Update(uint)"/> method, to update any internal
    /// state that this renderer may have for itself, or to modify values within the user's visualization (i.e. before a
    /// <see cref="Draw"/> occurs).
    /// </summary>
    ///
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time, directly from MonoGame.</param>
    /// <param name="willDoUserUpdate">
    /// Whether or not the user's <see cref="Visualization.Update"/> method will be called after this method is
    /// complete (it is not called on every frame, see <see cref="FrameDelay"/>).
    /// </param>
    ///
    /// <remarks>
    /// <para>
    /// The reason for both <see cref="PreUpdate"/> and <see cref="PostUpdate"/> to exist is to provide flexibility when
    /// implementing a renderer.
    /// </para>
    /// <para>
    /// The base implementation of this method does nothing.
    /// </para>
    /// </remarks>
    ///
    /// <seealso cref="PostUpdate(V, GameTime, bool)"/>
    /// <seealso cref="FrameDelay"/>
    protected internal virtual void PreUpdate(V userViz, GameTime gameTime, bool willDoUserUpdate)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame, after the user's <see cref="Visualization.Update(uint)"/> method, to update any internal
    /// state that this renderer may have for itself, or to modify values within the user's visualization (i.e. before a
    /// <see cref="Draw"/> occurs).
    /// </summary>
    ///
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time, directly from MonoGame.</param>
    /// <param name="didUserUpdate">
    /// Whether or not the user's <see cref="Visualization.Update"/> method was called before this method was called (it
    /// is not called on every frame, see <see cref="FrameDelay"/>).
    /// </param>
    ///
    /// <remarks><inheritdoc cref="PreUpdate(V, GameTime, bool)" path="/remarks"/></remarks>
    ///
    /// <seealso cref="PreUpdate(V, GameTime, bool)"/>
    /// <seealso cref="FrameDelay"/>
    protected internal virtual void PostUpdate(V userViz, GameTime gameTime, bool didUserUpdate)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to do any user-input handling that this renderer might want to do.
    /// </summary>
    ///
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time.</param>
    ///
    /// <remarks>
    /// The base implementation of this method does nothing.
    /// </remarks>
    protected internal virtual void HandleInput(V userViz, GameTime gameTime)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to draw the current state of the attached visualization to the screen.
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="didUserUpdate">
    /// Whether or not the user's <see cref="Visualization.Update"/> method was called during this frame.
    /// </param>
    protected internal abstract void Draw(V userViz, GameTime gameTime, bool didUserUpdate);

    /// <summary>
    /// Called right before the underlying MonoGame instance exits. This provides a single place to perform any state
    /// cleanup (e.g. un-redirecting the console) before control returns back to the user's main program. This method is
    /// called in a <see langword="finally"/> block should any exceptions happen.
    /// </summary>
    protected internal virtual void Cleanup()
    {
        // Default implementation does nothing.
    }

    #endregion

    #region User event handling

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserPause"/> event is fired. Its default behavior
    /// is simply to call the underlying control method, <see cref="Pause"/>.
    /// </summary>
    protected virtual void HandleUserPause() => Pause();

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserResume"/> event is fired. Its default behavior
    /// is simply to call the underlying control method, <see cref="Resume" />.
    /// </summary>
    protected virtual void HandleUserResume() => Resume();

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserStepForward"/> event is fired. Its default
    /// behavior is to check if the renderer is currently paused, and if so, to call the underlying control method,
    /// <see cref="StepForward"/>.
    /// </summary>
    protected virtual void HandleUserStepForward()
    {
        if (IsPaused) StepForward();
    }

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserStepBackward"/> event is fired. Its default
    /// behavior is to check if the renderer is currently paused, and if so, to call the underlying control method,
    /// <see cref="StepBackward"/>.
    /// </summary>
    protected virtual void HandleUserStepBackward()
    {
        if (IsPaused) StepBackward();
    }

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserExit"/> event is fired. Its default behavior is
    /// simply to call the underlying control method, <see cref="Exit"/>.
    /// </summary>
    protected virtual void HandleUserExit() => Exit();


    /// <summary>Pauses playback.</summary>
    protected void Pause() => IsPaused = true;

    /// <summary>Resumes playback.</summary>
    protected void Resume() => IsPaused = false;

    /// <summary>
    /// Steps playback forwards by one frame.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the renderer is not already paused.</exception>
    protected void StepForward()
    {
        EnsureStarted();

        if (!IsPaused)
            throw new InvalidOperationException("Renderer must be paused before single-stepping can occur.");

        CurrentFrame++;
        currentRunner.DoUserUpdate();
    }

    /// <summary>
    /// Steps playback backwards by one frame.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the renderer is not already paused.</exception>
    /// <remarks>
    /// This method doesn't do anything particularly special. All it does is decrement the current frame counter and
    /// call the user's update method.
    /// </remarks>
    protected void StepBackward()
    {
        EnsureStarted();

        if (!IsPaused)
            throw new InvalidOperationException("Renderer must be paused before single-stepping can occur.");

        CurrentFrame--;
        currentRunner.DoUserUpdate();
    }

    /// <summary>
    /// Closes the renderer.
    /// </summary>
    protected void Exit()
    {
        EnsureStarted();
        currentRunner.Exit();
    }


    [StackTraceHidden]
    [MemberNotNull(nameof(currentRunner))]
    private void EnsureStarted()
    {
        if (currentRunner is null)
            throw new InvalidOperationException("Cannot control playback before playback has started.");
    }

    #endregion
}
