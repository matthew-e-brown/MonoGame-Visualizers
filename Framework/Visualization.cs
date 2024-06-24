namespace TrentCOIS.Tools.Visualization;

using System;
using TrentCOIS.Tools.Visualization.Input;

/// <summary>
/// The base for all animations and visualizations.
///
/// <para>
/// This class is what is inherited by students or by assignment starter code. It provides a small set of intuitively
/// named methods within which students can write their code.
/// </para>
/// </summary>
public abstract class Visualization
{
    /// <summary>
    /// Whether or not this visualization has started yet.
    /// </summary>
    public bool HasStarted { get; internal set; }

    /// <summary>
    /// Retrieves information about currently pressed keyboard keys, mouse buttons and position, and so on.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// This property may be accessed during the <see cref="HandleInput"/> method (or the <see cref="Update"/> method,
    /// should it really by necessary) to query the current state of the user's input.
    /// </para>
    /// <para>
    /// It may also be accessed during initialization (in your constructor) to listen for
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/">events</see>,
    /// should you be familiar with those. Note that these events will be fired <b>before</b> <see cref="HandleInput"/>
    /// runs. See the documentation within <see cref="InputManager"/> for details.
    /// </para>
    /// </remarks>
    protected internal InputManager UserInput { get; }


    /// <summary>
    /// Creates a new base Game instance.
    /// </summary>
    public Visualization()
    {
        UserInput = new InputManager();
    }


    /// <summary>
    /// This method is called automatically on every frame. This is where your visualization/animation/game's state
    /// should be updated.
    /// </summary>
    ///
    /// <param name="currentFrame">The current tick/frame number/timestamp.</param>
    ///
    /// <remarks>
    /// <para>
    /// Overriding this method is optional, since some visualizations may only wish to update when keys are pressed or
    /// mice are clicked.
    /// </para>
    /// <para>
    /// The base implementation of this method does nothing.
    /// </para>
    /// </remarks>
    protected internal virtual void Update(uint currentFrame)
    {
        // Default implementation does nothing.
    }


    /// <summary>
    /// This method is called automatically on every <b>real frame</b>. This is user-input (e.g., key presses and mouse
    /// clicks) should be handled.
    /// </summary>
    ///
    /// <param name="deltaTime">
    /// How much real time has passed since the last time this method was called. Can be used to determine how far a
    /// mouse has moved, for example.
    /// </param>
    /// <param name="totalTime">
    /// The total amount of real time that has passed since this visualization started running.
    /// </param>
    ///
    /// <remarks>
    /// Since the visualization's speed is configurable with <see cref="Renderer{V}.FrameDelay"/> (and can even be
    /// single-stepped) <see cref="Update"/> is not necessarily called on every single <b>frame;</b> it merely refers to
    /// its ticks as "frames" for the sake of simplicity. User-input, however, needs to be handled at the full 60 FPS,
    /// otherwise things will feel very sluggish (or, when paused/single-stepping, completely unresponsive).
    /// </remarks>
    protected internal virtual void HandleInput(TimeSpan deltaTime, TimeSpan totalTime)
    {
        // Default implementation does nothing.
    }


    #region User events

    /// <summary>
    /// Handles a single playback request from the user (i.e. Play, Pause, Step, and so on).
    /// </summary>
    internal delegate void PlaybackRequestHandler();

    internal event PlaybackRequestHandler? UserPause;
    internal event PlaybackRequestHandler? UserResume;
    internal event PlaybackRequestHandler? UserStepForward;
    internal event PlaybackRequestHandler? UserStepBackward;
    internal event PlaybackRequestHandler? UserExit;


    /// <summary>
    /// Pauses the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Pause()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        UserPause?.Invoke();
    }


    /// <summary>
    /// Resumes the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Resume()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        UserResume?.Invoke();
    }


    /// <summary>
    /// Advances this visualization by a single frame.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void SingleStepForward()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Attempted to single-step a visualization that hasn't started yet.");
        UserStepForward?.Invoke();
    }


    /// <summary>
    /// Steps this visualization backwards by a single frame.
    /// </summary>
    /// <remarks>
    /// This method is not actually all that special; all it does is decrement the <c>currentFrame</c> counter and
    /// trigger a call to <see cref="Update"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void SingleStepBackward()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Attempted to single-step a visualization that hasn't started yet.");
        UserStepBackward?.Invoke();
    }


    /// <summary>
    /// Stops visualization playback and closes the window. Program execution will continue after
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Exit()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Attempted to exit a visualization that hasn't been started yet.");
        UserExit?.Invoke();
    }

    #endregion
}
