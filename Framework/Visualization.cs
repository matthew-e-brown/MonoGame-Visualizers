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
    /// A reference to the <see cref="Renderer"/> that is currently overseeing the execution of this "game." Will be
    /// <c>null</c> until the visualization is run.
    /// </summary>
    /// <seealso cref="Run()"/>
    /// <seealso cref="Run(RenderOptions)"/>
    internal Renderer? renderer;


    /// <summary>
    /// Creates a new base Game instance.
    /// </summary>
    public Visualization()
    { }


    /// <summary>
    /// Opens a window for this visualization and runs it.
    /// </summary>
    ///
    /// <exception cref="InvalidOperationException">If this visualization has already been started.</exception>
    public void Run() => Run(RenderOptions.DefaultOptions);

    /// <summary>
    /// Opens a window for this visualization and runs it.
    /// </summary>
    ///
    /// <param name="renderOptions">Settings to initialize the renderer with.</param>
    ///
    /// <exception cref="InvalidOperationException">If this visualization has already been started.</exception>
    public void Run(RenderOptions? renderOptions)
    {
        if (renderer is not null)
            throw new InvalidOperationException("Attempted to start a visualization that has already started.");

        renderer = new Renderer(this, renderOptions);
        renderer.Run();
    }


    /// <summary>
    /// This method is called once the graphics window has been initialized. Any rendering-related setup that couldn't
    /// be done in the constructor should go here. This includes things like the loading of sprites and fonts and the
    /// initialization of any secondary <see cref="Surface">Surfaces</see>.
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
    public virtual void LoadContent()
    {
        // Default implementation does nothing.
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
    protected internal virtual void Update()
    {
        // Default implementation does nothing.
    }


    /// <summary>
    /// This method is called automatically <b>once per frame,</b> and is where any logic that has to do with
    /// interactivity should go.
    /// </summary>
    /// <param name="gameTime">The amount of time that has passed since the visualization began.</param>
    /// <param name="inputs">A game component for querying the state and changes of the user's input.</param>
    protected internal virtual void HandleInput(TimeSpan gameTime, InputManager inputs)
    {
        // Default implementation does nothing.
    }


    /// <summary>
    /// Called <b>once per frame</b> to draw the current state of this visualization to the screen.
    /// </summary>
    ///
    /// <remarks>
    /// This method should <b>only</b> handle drawing. Put code related to logic/updates in the <see cref="Update" />
    /// method instead.
    /// </remarks>
    ///
    /// <param name="screen">A drawable surface that represents the main window.</param>
    protected internal abstract void Draw(Surface screen);


    /// <summary>
    /// Pauses the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Pause()
    {
        if (renderer is null)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        renderer.IsPaused = true;
    }

    /// <summary>
    /// Resumes the automatic updating of this visualization.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Resume()
    {
        if (renderer is null)
            throw new InvalidOperationException("Attempted to resume a visualization that hasn't started yet.");
        renderer.IsPlaying = true;
    }

    /// <summary>
    /// Stops visualization playback and closes the window. Program execution will continue after
    /// </summary>
    /// <exception cref="InvalidOperationException">If this visualization hasn't been started yet.</exception>
    public void Exit()
    {
        if (renderer is null)
            throw new InvalidOperationException("Attempted to exit a visualization that hasn't been started yet.");
        renderer.Exit();
        renderer.Dispose();
        renderer = null;
    }
}
