namespace TrentCOIS.Tools.Visualization;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using TrentCOIS.Tools.Visualization.Input;


/// <summary>
/// A level of abstraction for performing the actual drawing for a <see cref="Visualization"/>. Implementations of this
/// interface may have their own internal state, so this class has its own <see cref="PreUpdate"/> method.
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

    /// <summary>Whether or not the visualization is currently playing.</summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPlaying { get; set; }

    /// <summary>Whether or not the visualization is currently paused.</summary>
    /// <seealso cref="IsPaused"/>
    public bool IsPaused { get => !IsPlaying; set => IsPlaying = !value; }


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
        Graphics = null!; // This is set by the internal renderer before initialization.
        IsPaused = true;
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

        visualization.UserInput.AttachToGame(runner);

        currentRunner = runner; // Keep reference so we can access methods.
        runner.Run();           // Run the game
        currentRunner = null;   // Ensure dispose runs properly (I don't trust it if we keep a reference around)
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
    /// <param name="input">The input manager for the visualization, to establish event listeners.</param>
    public virtual void Initialize(V userViz, InputManager input)
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
    /// Called once per frame, before the user's <see cref="Visualization.Update(uint)"/> method, to update any internal
    /// state that this renderer may have for itself, or to modify values within the user's visualization (i.e. before a
    /// <see cref="Draw"/> occurs).
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time, directly from MonoGame.</param>
    /// <param name="frameNum">The frame number of the user's visualization that is about to be processed.</param>
    /// <param name="willDoUserUpdate">
    /// Whether or not the user's <see cref="Visualization.Update"/> method will be called after this method is
    /// complete (it is not called on every frame, see <see cref="FrameDelay"/>).
    /// </param>
    /// <remarks>
    /// Both this method and <see cref="PostUpdate"/> occur entirely before <see cref="Draw"/>. They exist to provide
    /// flexibility when implementing a visualization.
    /// </remarks>
    /// <seealso cref="PostUpdate(V, GameTime, uint, bool)"/>
    /// <seealso cref="FrameDelay"/>
    public virtual void PreUpdate(V userViz, GameTime gameTime, uint frameNum, bool willDoUserUpdate)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame, after the user's <see cref="Visualization.Update(uint)"/> method, to update any internal
    /// state that this renderer may have for itself, or to modify values within the user's visualization (i.e. before a
    /// <see cref="Draw"/> occurs).
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time, directly from MonoGame.</param>
    /// <param name="frameNum">The frame number of the user's visualization that was just processed.</param>
    /// <param name="didUserUpdate">
    /// Whether or not the user's <see cref="Visualization.Update"/> method was called before this method was called (it
    /// is not called on every frame, see <see cref="FrameDelay"/>).
    /// </param>
    /// <remarks>
    /// Both this method and <see cref="PreUpdate"/> occur entirely before <see cref="Draw"/>. They exist to provide
    /// flexibility when implementing a visualization.
    /// </remarks>
    /// <seealso cref="PreUpdate(V, GameTime, uint, bool)"/>
    /// <seealso cref="FrameDelay"/>
    public virtual void PostUpdate(V userViz, GameTime gameTime, uint frameNum, bool didUserUpdate)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to do any user-input handling that this renderer might want to do.
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="input">The <see cref="InputManager"/> instance from the user's visualization.</param>
    public virtual void HandleInput(V userViz, GameTime gameTime, InputManager input)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Called once per frame to draw the current state of the attached visualization to the screen.
    /// </summary>
    /// <param name="userViz">A reference to the user's visualization.</param>
    /// <param name="gameTime">The current game time.</param>
    public abstract void Draw(V userViz, GameTime gameTime);

    #endregion

    #region User event handling

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserPause"/> event is fired. Its default behavior
    /// is simply to call the <see cref="Pause">underlying control function</see>, but this can be overridden.
    /// </summary>
    protected virtual void HandleUserPause() => Pause();

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserResume"/> event is fired. Its default behavior
    /// is simply to call the <see cref="Resume">underlying control function</see>, but this can be overridden.
    /// </summary>
    protected virtual void HandleUserResume() => Resume();

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserStepForward"/> event is fired. Its default
    /// behavior is simply to call the <see cref="StepForward">underlying control function</see> (if the renderer is
    /// already paused), but this can be overridden.
    /// </summary>
    protected virtual void HandleUserStepForward()
    {
        if (IsPaused) StepForward();
    }

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserStepBackward"/> event is fired. Its default
    /// behavior is simply to call the <see cref="StepBackward">underlying control function</see> (if the renderer is
    /// already paused), but this can be overridden.
    /// </summary>
    protected virtual void HandleUserStepBackward()
    {
        if (IsPaused) StepBackward();
    }

    /// <summary>
    /// The method that is run whenever the <see cref="Visualization.UserExit"/> event is fired. Its default behavior is
    /// simply to call the <see cref="Exit">underlying control function</see>, but this can be overridden.
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

        currentRunner.CurrentFrame++;
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

        currentRunner.CurrentFrame--;
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
