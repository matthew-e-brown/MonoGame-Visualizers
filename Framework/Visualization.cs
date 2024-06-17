namespace TrentCOIS.Tools.Visualization;

using System;
using Microsoft.Xna.Framework;
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
    /// A reference to the <see cref="GameRunner"/> that is currently overseeing the execution of this "game."
    /// </summary>
    /// <seealso cref="Start()"/>
    internal GameRunner Runner { get; set; }

    private bool isStarted;

    /// <summary>
    /// An abstraction for the user's keyboard and mouse input.
    /// </summary>
    protected internal InputManager UserInput { get; }

    /// <summary>
    /// The class responsible for actually drawing this visualization to the screen.
    /// </summary>
    protected internal abstract IRenderer Renderer { get; }


    /// <summary>
    /// Creates a new base Game instance with the default <see cref="RunOptions"/>.
    /// </summary>
    public Visualization() : this(RunOptions.DefaultOptions)
    { }

    /// <summary>
    /// Creates a new base Game instance with the provided <see cref="RunOptions"/>.
    /// </summary>
    public Visualization(RunOptions? options)
    {
        Runner = new GameRunner(this, options);
        UserInput = new InputManager(Runner);
        isStarted = false;
    }


    /// <summary>
    /// Opens the window for this visualization and runs it.
    /// </summary>
    public void Start()
    {
        isStarted = true;
        Runner.Run();
    }


    /// <summary>
    /// This method is called automatically <b>once per tick</b> (i.e. based on the chosen frame-speed or when
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
    /// This method is called automatically <b>once per frame,</b> and is where any logic that has to do with
    /// interactivity should go.
    /// </summary>
    /// <param name="gameTime">The amount of real time that has passed since last time this method ran.</param>
    protected internal virtual void HandleInput(GameTime gameTime)
    {
        // Default implementation does nothing.
    }


    /// <summary>
    /// Pauses the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Pause()
    {
        if (!isStarted)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        Runner.IsPaused = true;
    }


    /// <summary>
    /// Resumes the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Resume()
    {
        if (!isStarted)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        Runner.IsPlaying = true;
    }


    /// <summary>
    /// Advances this visualization by a single frame.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void StepForward()
    {
        if (!isStarted)
            throw new InvalidOperationException("Attempted to single-step a visualization that hasn't started yet.");

        if (Runner.IsPaused)
            Runner.SingleStepForward();
    }


    /// <summary>
    /// Steps this visualization backwards by a single frame.
    /// </summary>
    /// <remarks>
    /// This method is not actually all that special; all it does is decrement the <c>currentFrame</c> counter and
    /// trigger a call to <see cref="Update"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void StepBackward()
    {
        if (!isStarted)
            throw new InvalidOperationException("Attempted to single-step a visualization that hasn't started yet.");

        if (Runner.IsPaused)
            Runner.SingleStepBackward();
    }


    /// <summary>
    /// Stops visualization playback and closes the window. Program execution will continue after
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Exit()
    {
        if (!isStarted)
            throw new InvalidOperationException("Attempted to exit a visualization that hasn't been started yet.");
        Runner.Exit();
        Runner.Dispose();
    }
}
