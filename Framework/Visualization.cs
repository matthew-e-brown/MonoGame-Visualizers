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
    /// An abstraction for the user's keyboard and mouse input.
    /// </summary>
    protected internal InputManager UserInput { get; }


    /// <summary>
    /// Creates a new base Game instance.
    /// </summary>
    public Visualization()
    {
        UserInput = new InputManager();
    }


    /// <summary>
    /// This method is called automatically once <b>per tick</b> (i.e., based on the chosen frame-speed or when
    /// single-stepping). This is where your visualization's state should be updated.
    /// </summary>
    ///
    /// <remarks>
    /// Implementing this method is optional, since some visualizations may only wish to update when keys are pressed or
    /// mice are clicked.
    /// </remarks>
    ///
    /// <param name="currentFrame">The number of the frame currently being processed.</param>
    protected internal virtual void Update(uint currentFrame)
    {
        // Default implementation does nothing.
    }


    /// <summary>
    /// This method is called automatically once <b>per frame</b> (as opposed to per <i>tick</i>), and is where any
    /// logic that has to do with interactivity should go.
    /// </summary>
    /// <param name="time">The total amount of time that has passed since this visualization started running.</param>
    protected internal virtual void HandleInput(TimeSpan time)
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
