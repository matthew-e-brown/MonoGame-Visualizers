namespace TrentCOIS.Tools.Visualization.Input;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// An event handler for responding to a single key being pressed or released.
/// </summary>
/// <param name="key">The key that was pressed or released.</param>
public delegate void KeyboardEventHandler(Key key);

/// <summary>
/// An event handler for responding to a single mouse button being pressed or released.
/// </summary>
/// <param name="button">The mouse button that was pressed or released.</param>
public delegate void MouseButtonEventHandler(MouseButton button);

/// <summary>
/// An event handler for responding to movement of the mouse wheel.
/// </summary>
/// <param name="vertical">The change in the mouse wheel's vertical position (the main axis).</param>
/// <param name="horizontal">The change in the mouse wheel's horizontal position (side-to-side).</param>
public delegate void MouseWheelEventHandler(int vertical, int horizontal);


/// <summary>
/// A game component that handles tracking user input and provides several helpful utilities. These include several
/// methods for querying the current state of a key or mouse button,
/// </summary>
public class InputManager
{
    /// <summary>
    /// The actual component that can be attached to a <see cref="Game"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Just like with the separation between <see cref="Visualization"/> and <see cref="GameRunner"/>, composition is
    /// used to hide the extra public/protected members that are inherited from the base class. That way, students'
    /// Intellisense is not filled with extra properties/methods that they shouldn't be calling.
    /// </para>
    /// <para>
    /// his component is what is actually added to the <see cref="Game.Components"/> collection. As such, if choosing to
    /// inherit from <see cref="InnerGameComponent"/>, the derived class it should handle calling this class's
    /// <see cref="Update"/> method (and/or its <see cref="Initialize"/> method).
    /// </para>
    /// </remarks>
    /// <seealso cref="InnerGameComponent"/>
    protected internal GameComponent ComponentInstance { get; }


    /// <summary>The current state of the user's mouse.</summary>
    protected MouseState CurrMouse { get; private set; }

    /// <summary>The state that the user's mouse was in during the last frame.</summary>
    protected MouseState PrevMouse { get; private set; }

    /// <summary>The current state of the user's keyboard.</summary>
    protected KeyboardState CurrKeys { get; private set; }

    /// <summary>The state that the user's keyboard was in during the last frame.</summary>
    protected KeyboardState PrevKeys { get; private set; }

    /// <summary>
    /// How long a key must be held down for before it starts repeating keypresses.
    /// </summary>
    public TimeSpan KeyHoldFirstDelay { get; set; } = new(500 * TimeSpan.TicksPerMillisecond);

    /// <summary>
    /// How long a held key should wait before triggering a second event.
    /// </summary>
    public TimeSpan KeyHoldRepeatDelay { get; set; } = new(100 * TimeSpan.TicksPerMillisecond);


    // Cached results from KeyboardState.GetPressedKeys(). These are handy to have when iterating, since we can loop
    // over _just_ the pressed keys, instead of checking all 150+ possible keys.
    private readonly Keys[] currPressed;
    private readonly Keys[] prevPressed;
    private int currKeyCount;
    private int prevKeyCount;

    // Keeps track of when a given key is allowed to be pressed next.
    // [FIXME] Switch this out with a more performant, index-based solution.
    private readonly Dictionary<Keys, TimeSpan> keyHoldTimers;


    /// <summary>
    /// This event is fired at the start of a frame once for each mouse button that was pressed on that frame.
    /// </summary>
    public event MouseButtonEventHandler? MousePressed;

    /// <summary>
    /// This event is fired at the start of a frame once for each mouse button that was released on that frame.
    /// </summary>
    public event MouseButtonEventHandler? MouseReleased;

    /// <summary>
    /// This event is fired at the start of a frame once for every key that was pressed on that frame.
    /// </summary>
    public event KeyboardEventHandler? KeyPressed;

    /// <summary>
    /// This event is fired at the start of a frame once for every key that was released on that frame.
    /// </summary>
    public event KeyboardEventHandler? KeyReleased;

    /// <summary>
    /// This event is fired at the start of a frame whenever the scroll-wheel has moved.
    /// </summary>
    public event MouseWheelEventHandler? MouseScroll;


    /// <summary>The X-coordinate of the mouse's current position.</summary>
    public int MouseX => CurrMouse.X;
    /// <summary>The Y-coordinate of the mouse's current position.</summary>
    public int MouseY => CurrMouse.Y;
    /// <summary>The mouse's current position.</summary>
    public Point MousePos => CurrMouse.Position;

    /// <summary>The X-coordinate of the mouse's position from the previous frame.</summary>
    public int LastMouseX => PrevMouse.X;
    /// <summary>The Y-coordinate of the mouse's position from the previous frame.</summary>
    public int LastMouseY => PrevMouse.Y;
    /// <summary>The mouse's position from the previous frame.</summary>
    public Point LastMousePos => PrevMouse.Position;

    /// <summary>How far the mouse moved left-to-right between this frame and the last frame.</summary>
    public int MouseDeltaX => CurrMouse.X - PrevMouse.X;
    /// <summary>How far the mouse moved up or down between this frame and the last frame.</summary>
    public int MouseDeltaY => CurrMouse.Y - PrevMouse.Y;
    /// <summary>How far the mouse moved between this frame and the last frame.</summary>
    public (int, int) MouseDelta => (CurrMouse.X - PrevMouse.X, CurrMouse.Y - PrevMouse.Y);

    /// <summary>How far the mouse-wheel was scrolled between this frame and the last frame.</summary>
    public int ScrollDistance => CurrMouse.ScrollWheelValue - PrevMouse.ScrollWheelValue;
    /// <summary>How far the mouse-wheel was scrolled side-to-side between this frame and the last.</summary>
    public int ScrollDistanceHorizontal => CurrMouse.HorizontalScrollWheelValue - PrevMouse.HorizontalScrollWheelValue;


    /// <summary>
    /// Creates a new <see cref="InputManager"/> for the given MonoGame <see cref="Game"/> instance.
    /// </summary>
    /// <param name="game">The game to handle inputs for.</param>
    public InputManager(Game game)
    {
        ComponentInstance = new InnerGameComponent(this, game);
        PrevKeys = CurrKeys = new KeyboardState();
        PrevMouse = CurrMouse = new MouseState();

        // Allocate enough space to handle the maximum possible number of keys (totally unnecessary, but can't hurt).
        int nKeys = Enum.GetValues<Keys>().Length;
        currPressed = new Keys[nKeys];
        prevPressed = new Keys[nKeys];
        currKeyCount = 0;
        prevKeyCount = 0;

        keyHoldTimers = new Dictionary<Keys, TimeSpan>(nKeys);
    }


    /// <summary>
    /// Initializes this <see cref="InputManager"/>.
    /// </summary>
    /// <remarks>
    /// The default implementation of this method is to do nothing. This method is provided so that it may be overridden
    /// by any derived classes, since it <i>is</i> expected to be present on regular <see cref="GameComponent"/>
    /// classes.
    /// </remarks>
    protected virtual void Initialize()
    {
        // Default is to do nothing.
    }


    #region Update

    // Used to allow iterating over mouse buttons in a loop
    private static readonly MouseButton[] allMouseValues = Enum.GetValues<MouseButton>();

    /// <summary>
    /// Updates this component's internal state and fires any events.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void Update(GameTime gameTime)
    {
        // Copy the current "current" state into the "previous" state before refreshing
        PrevMouse = CurrMouse;
        PrevKeys = CurrKeys;
        Array.Copy(currPressed, prevPressed, currKeyCount);
        prevKeyCount = currKeyCount;

        CurrMouse = Mouse.GetState();
        CurrKeys = Keyboard.GetState();
        CurrKeys.GetPressedKeys(currPressed);
        currKeyCount = CurrKeys.GetPressedKeyCount();

        // Handle events once state has been updated
        HandleMouseEvents();
        HandleKeyboardEvents(gameTime.TotalGameTime);
    }


    private void HandleMouseEvents()
    {
        foreach (MouseButton button in allMouseValues)
        {
            bool prev = GetMouseDown(PrevMouse, button);
            bool curr = GetMouseDown(CurrMouse, button);
            switch (prev, curr)
            {
                case (false, true): MousePressed?.Invoke(button); break;
                case (true, false): MouseReleased?.Invoke(button); break;
            }
        }

        if (ScrollDistance > 0 || ScrollDistanceHorizontal > 0)
            MouseScroll?.Invoke(ScrollDistance, ScrollDistanceHorizontal);
    }


    private void HandleKeyboardEvents(TimeSpan currTime)
    {
        // Sort the range based on the numerical value of the enum instead of the arbitrary-ish order of KeyboardState.
        // The prevPressed array is already sorted, since we copied it from the old version of this array at the start
        // of the frame. This operation should be fairly fast, since it's only sorting the sub-slice of keys that the
        // user is actually pressing right now.
        currPressed.AsSpan(0, currKeyCount).Sort((a, b) => (int)a - (int)b);

        int newIdx = 0; // index for new keys
        int oldIdx = 0; // index for old keys

        // Since our list of keys are sorted, we can use the double-pointer approach to search for common entries.
        while (newIdx < currKeyCount && oldIdx < prevKeyCount)
        {
            Keys oldKey = prevPressed[oldIdx];
            Keys newKey = currPressed[newIdx];
            if (oldKey < newKey)
            {
                // if oldKey < newKey, then oldKey is not in the new array. If it was, it would have come before
                // whatever newKey we just ran into.
                KeyReleased?.Invoke((Key)oldKey);
                keyHoldTimers.Remove(oldKey);
                oldIdx++;
            }
            else if (oldKey > newKey)
            {
                // opposite: newKey is not in the old array, for the same reason as above.
                KeyPressed?.Invoke((Key)newKey);
                keyHoldTimers[newKey] = currTime + KeyHoldFirstDelay;
                newIdx++;
            }
            else
            {
                // otherwise, the key is in both arrays and is being held down.
                newIdx++;
                oldIdx++;

                // Check if this key needs to fire a second event from being held down
                if (currTime >= keyHoldTimers[newKey])
                {
                    KeyPressed?.Invoke((Key)newKey);
                    keyHoldTimers[newKey] = currTime + KeyHoldRepeatDelay;
                }
            }
        }

        // Once we're done the main loop, we're past the end of at least one of the arrays. That means that all the rest
        // of the remaining array are not in-common, and so their state has changed.

        while (oldIdx < prevKeyCount) // Anything that was pressed and is no longer
        {
            Keys key = prevPressed[oldIdx++];
            KeyReleased?.Invoke((Key)key);
            keyHoldTimers.Remove(key);
        }

        while (newIdx < currKeyCount) // Anything that wasn't pressed and now is
        {
            Keys key = currPressed[newIdx++];
            KeyPressed?.Invoke((Key)key);
            keyHoldTimers[key] = currTime + KeyHoldFirstDelay;
        }
    }

    #endregion

    #region Mouse helpers

    /// <summary>Checks whether or not the left mouse button is currently released (not being held down).</summary>
    /// <returns>
    /// <c>true</c> if the left mouse button was not in a pressed state at the start of this frame; <c>false</c> if it
    /// was.
    /// </returns>
    public bool MouseIsUp() => MouseIsUp(MouseButton.Left);

    /// <summary>Checks whether or not the left mouse button is currently pressed down.</summary>
    /// <returns>
    /// <c>true</c> if the left mouse button was in a pressed state at the start of this frame; <c>false</c> if it
    /// wasn't.
    /// </returns>
    public bool MouseIsDown() => MouseIsDown(MouseButton.Left);

    /// <summary>Checks whether or not the left mouse button was pressed down between this frame and the last.</summary>
    /// <returns>
    /// <c>true</c> if the left mouse button was released for the previous frame, but pressed at the start of this
    /// frame. <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasPressed() => MouseWasPressed(MouseButton.Left);

    /// <summary>Checks whether or not the left mouse button was released between this frame and the last.</summary>
    /// <returns>
    /// <c>true</c> if the left mouse button was pressed for the previous frame, but released at the start of this
    /// frame. <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasReleased() => MouseWasReleased(MouseButton.Left);

    /// <summary>
    /// Checks whether or not the left mouse button was pressed down on the previous frame and is still being held down.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the left mouse button was in a pressed state both at the start of the previous frame and at the
    /// start of this frame; <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasHeld() => MouseWasHeld(MouseButton.Left);


    /// <summary>Checks whether or not the given mouse button is currently released (not being held down).</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// <c>true</c> if the given mouse button was not in a pressed state at the start of this frame; <c>false</c> if it
    /// was.
    /// </returns>
    public bool MouseIsUp(MouseButton button) => !GetMouseDown(CurrMouse, button);

    /// <summary>Checks whether or not the given mouse button is currently pressed down.</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// <c>true</c> if the given mouse button was in a pressed state at the start of this frame; <c>false</c> if it
    /// wasn't.
    /// </returns>
    public bool MouseIsDown(MouseButton button) => GetMouseDown(CurrMouse, button);

    /// <summary>Checks whether or not the given mouse button was pressed down between this frame and the last.</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// <c>true</c> if the given mouse button was released for the previous frame, but pressed at the start of this
    /// frame. <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasPressed(MouseButton button) => GetMouseDown(CurrMouse, button) && !GetMouseDown(PrevMouse, button);

    /// <summary>Checks whether or not the given mouse button was released between this frame and the last.</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// <c>true</c> if the given mouse button was pressed for the previous frame, but released at the start of this
    /// frame. <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasReleased(MouseButton button) => !GetMouseDown(CurrMouse, button) && GetMouseDown(PrevMouse, button);

    /// <summary>
    /// Checks whether or not the given mouse button was pressed down on the previous frame and is still being held
    /// down.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// <c>true</c> if the given mouse button was in a pressed state both at the start of the previous frame and at the
    /// start of this frame; <c>false</c> otherwise.
    /// </returns>
    public bool MouseWasHeld(MouseButton button) => GetMouseDown(CurrMouse, button) && GetMouseDown(PrevMouse, button);

    #endregion

    #region Keyboard helpers

    /// <summary>Checks whether or not a keyboard key is currently released (not being held down).</summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <c>true</c> if the given key was not in a pressed state at the start of this frame; <c>false</c> if it was.
    /// </returns>
    public bool KeyIsUp(Key key) => !GetKeyDown(CurrKeys, key);

    /// <summary>Checks whether or not a keyboard key is currently pressed down.</summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <c>true</c> if the given key was in a pressed state at the start of this frame; <c>false</c> if it wasn't.
    /// </returns>
    public bool KeyIsDown(Key key) => GetKeyDown(CurrKeys, key);

    /// <summary>Checks whether or not a keyboard key was pressed down between this frame and the last.</summary>
    /// <param name="key">They key to check.</param>
    /// <returns>
    /// <c>true</c> if the given key was released for the previous frame, but pressed at the start of this frame.
    /// <c>false</c> otherwise.
    /// </returns>
    public bool KeyWasPressed(Key key) => GetKeyDown(CurrKeys, key) && !GetKeyDown(PrevKeys, key);

    /// <summary>Checks whether or not the given keyboard key was released between this frame and the last.</summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <c>true</c> if the given key was pressed for the previous frame, but released at the start of this frame.
    /// <c>false</c> otherwise.
    /// </returns>
    public bool KeyWasReleased(Key key) => !GetKeyDown(CurrKeys, key) && GetKeyDown(PrevKeys, key);

    /// <summary>
    /// Checks whether or not the given key was pressed down on the previous frame and is still being held
    /// down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <c>true</c> if the given key was in a pressed state both at the start of the previous frame and at the start of
    /// this frame; <c>false</c> otherwise.
    /// </returns>
    public bool KeyWasHeld(Key key) => GetKeyDown(CurrKeys, key) && GetKeyDown(PrevKeys, key);

    #endregion

    #region Internals

    /// <summary>
    /// Queries whether or not a given mouse button is or was <see cref="ButtonState.Pressed">pressed</see>.
    /// </summary>
    /// <param name="state">Which state to query for the mouse button.</param>
    /// <param name="button">The mouse button to query.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="button"/> was pressed in the provided <paramref name="state"/>; <c>false</c>
    /// otherwise.
    /// </returns>
    protected static bool GetMouseDown(MouseState state, MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => state.LeftButton == ButtonState.Pressed,
            MouseButton.Right => state.RightButton == ButtonState.Pressed,
            MouseButton.Middle => state.MiddleButton == ButtonState.Pressed,
            MouseButton.Button4 => state.XButton1 == ButtonState.Pressed,
            MouseButton.Button5 => state.XButton2 == ButtonState.Pressed,
            _ => throw new ArgumentException("Invalid mouse button", nameof(button)),
        };
    }

    /// <summary>
    /// Queries whether or not a given keyboard key is or was pressed down.
    /// </summary>
    /// <param name="state">Which state to query the key's state for.</param>
    /// <param name="key">The key to query for the state of.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="key"/> was pressed down in the provided <paramref name="state"/>;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// This method exists mostly for consistency with <see cref="GetMouseDown"/>. It also handles converting our
    /// <see cref="Key"/> enum into MonoGame's <see cref="Keys"/> enum.
    /// </remarks>
    protected static bool GetKeyDown(KeyboardState state, Key key)
    {
        return state.IsKeyDown((Keys)key);
    }


    /// <summary>
    /// The actual implementation of the <see cref="GameComponent"/> interface provided by MonoGame.
    /// </summary>
    /// <seealso cref="ComponentInstance">For an explanation as to why this class exists.</seealso>
    protected class InnerGameComponent : GameComponent
    {
        /// <summary>
        /// A reference to this composed component's parent.
        /// </summary>
        protected InputManager Parent { get; }

        /// <summary>
        /// Creates a new component for the given InputManager and game.
        /// </summary>
        /// <param name="parent">The <see cref="InputManager"/> this component is attached to.</param>
        /// <param name="game">The game that <paramref name="parent"/> is attached to.</param>
        public InnerGameComponent(InputManager parent, Game game) : base(game)
        {
            Parent = parent;
        }

        /// <summary>
        /// Calls the parent InputManager's <see cref="InputManager.Initialize"/> method.
        /// </summary>
        public override void Initialize() => Parent.Initialize();

        /// <summary>
        /// Calls the parent InputManager's <see cref="InputManager.Update(GameTime)"/> method.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public override void Update(GameTime gameTime) => Parent.Update(gameTime);
    }

    #endregion
}
