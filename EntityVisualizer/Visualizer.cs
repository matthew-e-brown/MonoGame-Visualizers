namespace TrentCOIS.Tools.Visualization.EntityViz;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;

/* ==================================================================================================================
 * NB: This class contains heavy usage of `sealed` and `private protected` to prevent students from accidentally
 * breaking anything when writing their main programs. Just to avoid potential headaches. This project is given to them
 * as part of the starter code, so they could come in here and tinker if they really want to.
 * ================================================================================================================== */

// [TODO] Switch from inheritance to composition for `Game`. Would like to prevent students from having access to `Game`
// fields like .Components, .Services, .GraphicsDevice, .LaunchParameters, etc.

/// <summary>
/// The main entrypoint of EntityVisualizer. This class handles user input, processes game-state updates, and draws to
/// the screen.
/// </summary>
public abstract class Visualizer : Game
{
    /// <summary>Which key the user should press to play/pause the visualization.</summary>
    public const Keys PlayKey = Keys.Space;

    /// <summary>Which key the user should press to single-step the visualization.</summary>
    public const Keys StepKey = Keys.Right;

    // --------------------------------------------------------------

    /// <summary>
    /// How large each tile is, as a multiple of the base 10x10-pixel size.
    /// </summary>
    public ushort TileScale { get; set; } = 3;

    /// <summary>
    /// How wide the main area is, in in-game tiles. Should be an odd number so that the center tiles actually display
    /// in the center.
    /// </summary>
    public ushort ArenaWidth { get; set; } = 15;

    /// <summary>
    /// How tall the main area is, in in-game tiles. Should be an odd number so that the center tiles actually display
    /// in the center.
    /// </summary>
    public ushort ArenaHeight { get; set; } = 15;

    /// <summary>
    /// The number of milliseconds' delay between each step of the simulation.
    /// </summary>
    public long TickDelayMS
    {
        get => tickDelay.Milliseconds;
        set => tickDelay = new TimeSpan(value * TimeSpan.TicksPerMillisecond);
    }

    /// <summary>
    /// Whether or not playback is currently enabled.
    /// </summary>
    public bool IsPlaying { get; set; }

    /// <summary>
    /// Whether or not playback is currently on pause.
    /// </summary>
    public bool IsPaused { get => !IsPlaying; set => IsPlaying = !value; }

    /// <summary>
    /// The current game timestamp, or the number of "ticks" that have elapsed since the start of the simulation. Useful
    /// for delaying an entity's actions for a certain number of frames. Also useful for logging purposes.
    /// </summary>
    ///
    /// <remarks>
    /// For all intents and purposes, this counter is 1-based. It starts at zero, but it is incremented before the first
    /// time the user's <see cref="Update()"/> method is called. That way, tick number "zero" is the one seen before any
    /// stepping occurs.
    /// </remarks>
    public uint CurrentTimestamp { get; private protected set; } = 0;

    /// <summary>
    /// How far an entity is allowed to move horizontally under regular conditions.
    /// </summary>
    public (float, float) EntityXRange { get => (-(ArenaWidth * 0.5f - 0.5f), ArenaWidth * 0.5f - 0.5f); }

    /// <summary>
    /// How far an entity is allowed to move vertically under regular conditions..
    /// </summary>
    public (float, float) EntityYRange { get => (-(ArenaHeight * 0.5f - 0.5f), ArenaHeight * 0.5f - 0.5f); }

    // --------------------------------------------------------------

    #region Internal fields


    private protected int windowWidth;
    private protected int windowHeight;

    private protected GraphicsDeviceManager graphics;
    private protected SpriteBatch spriteBatch;
    private protected FontSystem fontSystem;

    private protected bool contentLoaded = false;       // So we don't accidentally access any still-null fields

    private protected Texture2D basicQuad;              // 1x1 white quad, stretched and tinted to draw boxes

    // Direct access to sprite objects loaded from assets
    private protected Dictionary<string, Sprite> Sprites { get; private set; }

    private protected TimeSpan tickDelay;               // How long to wait between each tick (see above)
    private protected TimeSpan lastTicked;              // When the last user tick happened

    private protected KeyboardState oldKeys;            // Current state of keys, for comparing against new inputs
    private protected TimeSpan stepKeyTimer;            // When the step-key is allowed to activate next.
    private protected static TimeSpan keyHoldDelay = new(100 * TimeSpan.TicksPerMillisecond);
    private protected static TimeSpan keyFirstDelay = new(500 * TimeSpan.TicksPerMillisecond);

    private protected const int basePixelSize = 10;
    private protected int TileSize => basePixelSize * TileScale;

    private protected RenderPanel mainPanel;            // Where the battle actually takes place.
    private protected RenderPanel textPanel;            // The area where messages logged to the screen get displayed.
    private protected RenderPanel infoPanel;            // The area displaying stats of the different entities.
    private protected const int panelSpacing = 10;      // How much space is drawn around the edge of each panel.

    private protected const int msgFontSize = 18;       // Font size for message panel
    private protected const int textLogPadding = 5;     // Padding inside the panel to give text breathing room.
    private protected const int msgLineSpacing = 5;     // Space between each message in the log.
    private protected const int infoBoxPadding = 4;     // Padding inside each entity's little box.

    // NB: message log padding is doubled for X direction in DrawTextPanel. It just looks a little better.

    private protected const string infoMsgIcon = "\U0001F514";  // U+1F514 "Bell" 🔔

    private protected int textPanelLineLimit;           // Measured and configured when font is loaded.
    private protected Queue<string> messageLines;       // The lines of text drawn in the text panel.
    private protected StringBuilder logBuilder;         // Queued lines are merged together and printed all at once.

    private protected bool forceStop = false;           // Like `IsPaused`, but cannot be resumed by the user.

    // NB: a queue is used for circular-buffer behaviour as opposed to avoid popping off the front of a List<string>
    // NB: StringBuilder is an instance member to avoid unnecessary reallocations

    private protected static Color MainBackgroundColor = new(32, 18, 8);
    private protected static Color DarkBackgroundColor = new(16, 9, 4);


    #endregion

    #region Constructors


    /// <summary>
    /// Creates a new, uninitialized visualizer with a window size of 1280x720 (720p).
    /// </summary>
    public Visualizer() : this(1280, 720)
    { }

    /// <summary>
    /// Creates a new, uninitialized visualizer with a given resolution.
    /// </summary>
    public Visualizer(int windowWidth, int windowHeight)
    {
        this.windowWidth = windowWidth;
        this.windowHeight = windowHeight;

        graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;

        Sprites = new();

        messageLines = new();
        logBuilder = new();

        TickDelayMS = 100; // 100ms default; can be overridden by the user with setter/initializer.
        lastTicked = TimeSpan.Zero;

        // Many fields of this class require the Graphics Device to be set up before they can be initialized. However,
        // MonoGame doesn't guarantee that graphics are set up until `LoadContent` runs; so we need to leave them as
        // null until then. Use `!` to lie to the compiler and say they aren't null. We could #pragma suppress CS8618,
        // but that would suppress it for *all* fields, not just the ones we're explicitly leaving out.

        spriteBatch = null!;
        fontSystem = null!;

        mainPanel = null!;
        textPanel = null!;
        infoPanel = null!;

        basicQuad = null!;
    }


    #endregion

    #region User methods


    /// <summary>
    /// Should handle all of the logic required to render the next frame/tick of the simulation.
    /// </summary>
    protected abstract void Update();

    /// <summary>
    /// Should return a sequence of all of the entities that are participating in this simulation and are to be drawn.
    /// </summary>
    protected abstract IEnumerable<Entity> GetEntities();


    // --------------------------------------------------------------


    /// <summary>
    /// Stops this simulation permanently.
    /// </summary>
    ///
    /// <remarks>
    /// To temporarily stop the simulation, toggle the <see cref="IsPlaying"/>/<see cref="IsPaused"/> properties (or
    /// press the <see cref="PlayKey">appropriate keyboard key</see>).
    /// </remarks>
    public void Stop()
    {
        IsPlaying = false;
        forceStop = true;
        LogMessage($"{infoMsgIcon} Game stopped!", true);
    }


    /// <summary>
    /// Prints a message to the simulator's message panel. Message will be logged with a timestamp indicating which tick
    /// it was called during.
    /// </summary>
    /// <param name="text">The message to print. Multiple lines are allowed.</param>
    protected void LogMessage(string text) => LogMessage(text, true);

    /// <summary>
    /// Prints a message to the simulator's message panel, with or without the included <c>[Tick XYZ]</c> at the start.
    /// </summary>
    /// <param name="text">The message to print. Multiple lines are allowed.</param>
    /// <param name="includeTimestamp">Enable/disables the <c>[Tick XYZ]</c> prefix on the message.</param>
    protected void LogMessage(string text, bool includeTimestamp)
    {
        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (contentLoaded && messageLines.Count > textPanelLineLimit)
                messageLines.Dequeue();

            // Leave the start un-trimmed so the user can do indenting if they'd like
            string trimmed = line.TrimEnd();
            if (includeTimestamp)
                trimmed = $"[Tick {CurrentTimestamp,5}] {trimmed}";

            messageLines.Enqueue(trimmed);
        }

        // No flag to set if panel isn't loaded yet. It will get drawn on the first frame automatically.
        if (contentLoaded)
            textPanel.Dirty = true;
    }


    #endregion

    #region Setup


    /// <summary>
    /// Sets up any game-state that depends on things like the main window being set up.
    /// </summary>
    ///
    /// <remarks>
    /// You do not need to call this method. It is executed automatically when calling <c>Run()</c>.
    /// </remarks>
    protected sealed override void Initialize()
    {
        // Configure window
        Window.AllowUserResizing = false;
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = windowWidth;
        graphics.PreferredBackBufferHeight = windowHeight;
        graphics.ApplyChanges();

        LogMessage($"{infoMsgIcon} Use the 'LogMessage' method in your Game class to print messages to this console.", false);
        LogMessage($"{infoMsgIcon} Press {PlayKey} to play/pause and {StepKey} to single-step forwards.", false);

        // This check is done here just in case the user wants to set it to paused before starting.
        if (IsPaused)
            LogMessage($"{infoMsgIcon} Playback paused. Press Space to resume.", false);

        // Initializing panels requires the GraphicsDevice to be setup, so we do that in LoadContent.

        base.Initialize();
    }

    /// <summary>
    /// Loads and configures any data that depends on the graphics device being set up.
    /// </summary>
    ///
    /// <remarks>
    /// You do not need to call this method. It is executed automatically when calling <c>Run()</c>, as well as whenever
    /// a graphics device reset occurs.
    /// </remarks>
    protected sealed override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Sprites = Sprite.LoadAll(GraphicsDevice);

        fontSystem = new FontSystem(new FontSystemSettings()
        {
            FontResolutionFactor = 4.0f,
            KernelWidth = 4,
            KernelHeight = 4,
            TextureWidth = FontSystemDefaults.TextureWidth * 2, // 2x on 2 sides = 4x increase in area
            TextureHeight = FontSystemDefaults.TextureHeight * 2,
        });

        var dllPath = Path.GetDirectoryName(typeof(Visualizer).Assembly.Location);
        var monoPath = Path.Join(dllPath, "Resources/Fonts/NotoSansMono_Condensed-Medium.ttf");
        var emojiPath = Path.Join(dllPath, "Resources/Fonts/NotoEmoji-Bold.ttf");
        fontSystem.AddFont(File.ReadAllBytes(monoPath));
        fontSystem.AddFont(File.ReadAllBytes(emojiPath));

        // --------------------------

        // The number of tiles they want, plus two on either side for the borders, multiplied by the tile scales, is how
        // large the main playfield is. The other two regions take up the remaining space.

        int mainWidth = (ArenaWidth + 2) * TileSize;
        int mainHeight = (ArenaHeight + 2) * TileSize;

        int infoLeftEdge = mainWidth + panelSpacing * 2;
        int infoWidth = windowWidth - mainWidth - panelSpacing * 3;

        int textTopEdge = mainHeight + panelSpacing * 2;
        int textHeight = windowHeight - mainHeight - panelSpacing * 3;

        mainPanel = new RenderPanel(GraphicsDevice, panelSpacing, panelSpacing, mainWidth, mainHeight);
        infoPanel = new RenderPanel(GraphicsDevice, infoLeftEdge, panelSpacing, infoWidth, mainPanel.Height);
        textPanel = new RenderPanel(GraphicsDevice, panelSpacing, textTopEdge, windowWidth - panelSpacing * 2, textHeight);

        mainPanel.DrawFn = RedrawMainPanel;
        infoPanel.DrawFn = RedrawInfoPanel;
        textPanel.DrawFn = RedrawTextPanel;

        // --------------------------

        // To draw lines, we need a basic quad.
        basicQuad = new Texture2D(GraphicsDevice, 1, 1);
        basicQuad.SetData(new Color[] { Color.White }); // Can make use of spriteBatch tinting to get colours

        // --------------------------

        // Measure how tall a line of text is in this font for the message panel to determine the number of lines we can
        // display at the bottom.
        var msgFont = fontSystem.GetFont(msgFontSize);
        int availableHeight = textHeight - textLogPadding * 2;

        // Not a perfect estimate, since there are less spaces than lines, but good enough to restrict the quantity of
        // our queue.
        int fullLineCount = availableHeight / (msgFont.LineHeight + msgLineSpacing);
        textPanelLineLimit = fullLineCount + 1; // Extra buffer line that will appear partially cut off on the top

        // --------------------------

        contentLoaded = true;
    }


    #endregion

    #region Update


    /// <summary>
    /// The internal MonoGame version of <see cref="Update()"/>.
    /// </summary>
    ///
    /// <remarks>
    /// This method is executed automatically every single frame. This is in contrast to the
    /// <see cref="Update()">parameterless version of <c>Update</c></see>, which is only called after a given
    /// <see cref="TickDelayMS">tick-delay</see> specified by the user (or when single-stepping).
    /// </remarks>
    protected sealed override void Update(GameTime gameTime)
    {
        var currTime = gameTime.TotalGameTime;

        HandleInput(currTime);

        if (IsPlaying && lastTicked + tickDelay <= currTime)
            CallUserUpdate(currTime);

        base.Update(gameTime);
    }

    /// <summary>
    /// Calls the student's version of Update (the zero-parameter version), increments tick count and timers, and sets
    /// panel dirty flags.
    /// </summary>
    /// <remarks>
    /// Called from both the main Update loop and the input handler (if playback is paused).
    /// </remarks>
    private void CallUserUpdate(TimeSpan currTime)
    {
        CurrentTimestamp += 1;
        Update();
        lastTicked = currTime;
        mainPanel.Dirty = true;
        infoPanel.Dirty = true;
    }


    private void HandleInput(TimeSpan currTime)
    {
        var newKeys = Keyboard.GetState();

        if (newKeys.IsKeyDown(Keys.Escape))
            Exit();

        // If they pressed play/pause on this frame, toggle playback (requires a full press to toggle, so no timer
        // needed).
        if (oldKeys.IsKeyUp(PlayKey) && newKeys.IsKeyDown(PlayKey) && !forceStop)
        {
            IsPlaying = !IsPlaying;

            if (IsPlaying)
                LogMessage($"{infoMsgIcon} Resumed.");
            else
                LogMessage($"{infoMsgIcon} Paused.");
        }

        // Otherwise, if we're currently paused, they're allowed to step.
        else if (!IsPlaying && newKeys.IsKeyDown(StepKey) && !forceStop)
        {
            TimeSpan? nextDelay = null; // if not-null, we should tick on this frame.

            // If they weren't holding down the key previously, this is a new press and we want to run their update
            // immediately. Otherwise, they're holding it down; advance forwards only if it's been long enough.
            if (oldKeys.IsKeyUp(StepKey)) nextDelay = keyFirstDelay;
            else if (currTime >= stepKeyTimer) nextDelay = keyHoldDelay;

            if (nextDelay is TimeSpan delay)
            {
                LogMessage($"{infoMsgIcon} Stepping into Tick {CurrentTimestamp}...", false);
                CallUserUpdate(currTime);
                stepKeyTimer = currTime + delay;
            }
        }

        // No matter what (even if they just paused), if they have since released the step key, they are now allowed to
        // press it again.
        if (newKeys.IsKeyUp(StepKey))
        {
            stepKeyTimer = currTime;
        }

        oldKeys = newKeys;
    }


    #endregion

    #region Drawing


    /// <summary>
    /// Computes an entity's (x, y) position on the mainPanel, as opposed to the from-the-middle position they store
    /// internally.
    /// </summary>
    private Vector2 GetEntityMainPosition(Entity entity)
    {
        Vector2 panelCenter = new(mainPanel.Width * 0.5f, mainPanel.Height * 0.5f);
        return panelCenter + entity.Position * TileSize;
    }


    // UNUSED: Could be helpful to draw lines between the info panel boxes and the main game board, for keeping track of
    // entities
    // --------
    // private (int, int) MainPanelToScreenCoords(Vector2 pos) => MainPanelToScreenCoords((int)pos.X, (int)pos.Y);
    // private (int, int) MainPanelToScreenCoords(int x, int y) => (x - panelSpacing, y - panelSpacing);
    // private (int, int) InfoPanelToScreenCoords(Vector2 pos) => InfoPanelToScreenCoords((int)pos.X, (int)pos.Y);
    // private (int, int) InfoPanelToScreenCoords(int x, int y) => (x - panelSpacing * 2 - mainPanel.Width, y - panelSpacing);


    private protected void RedrawMainPanel(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw walls by looping around the perimeter
        Sprite wall = Sprites["Buildings/Walls/Gray/WoodPanels"];
        Sprite ground1 = Sprites["Ground/Brown/Checkerboard"];
        Sprite ground2 = Sprites["Ground/Gray/YoshiCookie"];
        Sprite radius = Sprites["Other/Circle"];

        int tileCenterX = (ArenaWidth + 2) / 2;
        int tileCenterY = (ArenaHeight + 2) / 2;

        // point filtering instead of linear texture filtering for nearest-neighbour. we want non-premultiplied alpha so
        // that our tints will correctly transparentize the range circle.
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);

        // Draw the arena itself
        for (int y = 0; y < ArenaHeight + 2; y++)
        {
            for (int x = 0; x < ArenaWidth + 2; x++)
            {
                Sprite tile;
                if (x == 0 || y == 0 || x == ArenaWidth + 1 || y == ArenaHeight + 1)
                    tile = wall;
                else if (x == tileCenterX || y == tileCenterY)
                    tile = ground2;
                else
                    tile = ground1;

                int tx = x * TileSize;
                int ty = y * TileSize;
                tile.Draw(spriteBatch, tx, ty, TileSize, TileSize);
            }
        }

        // Loop over all entities that have a radius to them and draw their radii
        foreach (Entity entity in GetEntities())
        {
            // Draw the circle under them first if applicable
            if (entity is IHasRange rangedEntity)
            {
                int diam = (int)(rangedEntity.Range * 2 * TileSize);
                var sPos = GetEntityMainPosition(entity);
                radius.DrawCentered(spriteBatch, sPos, diam, diam, rangedEntity.RangeCircleColor);
            }
        }

        // Now loop over again and actually draw them
        foreach (Entity entity in GetEntities())
        {
            var lilGuy = Sprites[entity.SpriteName];
            var accPos = GetEntityMainPosition(entity);
            lilGuy.DrawCentered(spriteBatch, accPos, TileSize, TileSize);
        }

        spriteBatch.End();
    }


    private protected void RedrawInfoPanel(SpriteBatch spriteBatch, GameTime gameTime)
    {
        const int sp = infoBoxPadding;          // spacing within the mini boxes
        const int lh = 20;                      // line height
        const int bh = lh * 2 + sp * 3;         // box height
        int bw = (infoPanel.Width - sp) / 2;    // box width, depends on size of info panel

        var font = fontSystem.GetFont(lh);

        Sprite hpSprite2 = Sprites["UI/StatusBar/Heart.Wide.Full"];
        Sprite hpSprite1 = Sprites["UI/StatusBar/Heart.Wide.Half"];
        Sprite hpSprite0 = Sprites["UI/StatusBar/Heart.Wide.Empty"];

        // Sprite hpSprite2 = Sprites["UI/StatusBar/TwoHearts.2"];
        // Sprite hpSprite1 = Sprites["UI/StatusBar/TwoHearts.1"];
        // Sprite hpSprite0 = Sprites["UI/StatusBar/TwoHearts.0"];

        // For each unit, draw a little box with their stats.
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        int y = 0; // Increment as we draw
        int x = 0;

        foreach (Entity entity in GetEntities())
        {
            int innerMarginL = x + sp;
            int innerMarginR = x + bw - sp;
            int l1MarginTop = y + sp;
            int l2MarginTop = l1MarginTop + lh + sp;

            // Draw a background for the box
            spriteBatch.Draw(basicQuad, new Rectangle(x, y, bw, bh), DarkBackgroundColor);

            // Draw the "portrait"
            Sprite lilGuy = Sprites[entity.SpriteName];
            lilGuy.Draw(spriteBatch, new Rectangle(innerMarginL, l1MarginTop, lh, lh));

            // Draw the name a little to the side (double spacing)
            var textStr = entity.ToString();
            var textPos = new Vector2(innerMarginL + lh + sp * 2, l1MarginTop);
            spriteBatch.DrawString(font, textStr, textPos, Color.White);

            // Draw the box background again to cover any potentially long names, then draw the ID on the far right
            var idString = $" #{entity.ID:D2}";
            var idTextW = (int)Math.Ceiling(font.MeasureString(idString).X);

            int idCoverL = innerMarginR - idTextW - sp * 2; // double padding on the left
            int idTextL = innerMarginR - sp - idTextW;
            var idCoverRect = new Rectangle(idCoverL, l1MarginTop, idTextW + sp * 2, lh);
            spriteBatch.Draw(basicQuad, idCoverRect, DarkBackgroundColor);
            font.DrawText(spriteBatch, idString, new Vector2(idTextL, l1MarginTop), Color.White);

            // Draw the health, if applicable
            if (entity is IHasHP hpEntity)
            {
                int heartX = innerMarginL;
                for (int h = 0; h < hpEntity.MaxHP; h += 2)
                {
                    #pragma warning disable format
                    Sprite sprite;
                    if (hpEntity.HP >= h + 2)       sprite = hpSprite2;
                    else if (hpEntity.HP >= h + 1)  sprite = hpSprite1;
                    else                            sprite = hpSprite0;
                    #pragma warning restore format

                    // If this entity has 5 health, we will loop for 0, 2, 4. When we do the last iteration, if MaxHP < h +
                    // 2, the final one is only a single heart. So we crop the sprite in half.
                    int dw = lh; // dw = draw width, with cropping taken into account
                    if (hpEntity.MaxHP < h + 2)
                    {
                        dw /= 2;
                        sprite = sprite.Cropped(new Rectangle(0, 0, sprite.Width / 2, sprite.Height));
                    }

                    sprite.Draw(spriteBatch, new Rectangle(heartX, l2MarginTop, dw, lh));
                    heartX += dw + /* sp */ 0; // no spacing between hearts, since textures stack right beside one another
                }
            }

            // Draw the position; this one also gets a box behind it.
            var posString = $" ({entity.Position.X:F2}, {entity.Position.Y:F2})";
            var posTextW = (int)Math.Ceiling(font.MeasureString(posString).X);

            int posCoverL = innerMarginR - posTextW - sp * 2;
            int posTextL = innerMarginR - sp - posTextW;
            var posCoverRect = new Rectangle(posCoverL, l2MarginTop, posTextW + sp * 2, lh);
            spriteBatch.Draw(basicQuad, posCoverRect, DarkBackgroundColor);
            font.DrawText(spriteBatch, posString, new Vector2(posTextL, l2MarginTop), Color.White);

            // Advance to next box
            y += bh + sp;
            if (y + bh >= infoPanel.Height)
            {
                y = 0;
                x += bw + sp;
            }

            if (x >= infoPanel.Width)
                break;
        }

        spriteBatch.End();
    }


    private protected void RedrawTextPanel(SpriteBatch spriteBatch, GameTime gameTime)
    {
        const int pad = textLogPadding; // Spacing at the top and bottom of the message box

        var msgFont = fontSystem.GetFont(msgFontSize);

        // Collect all of the lines of text in the queue
        logBuilder.Clear();
        foreach (string line in messageLines)
        {
            logBuilder.Append(line);
            logBuilder.Append('\n');
        }
        logBuilder.Remove(logBuilder.Length - 1, 1); // Trim last newline

        // Convert to one big string and measure how large it'll be
        var fullMessageText = logBuilder.ToString();
        var fullBlockHeight = msgFont.MeasureString(fullMessageText, lineSpacing: msgLineSpacing).Y;

        // We want it aligned to bottom left, so shift that point down by its height
        var textPos = new Vector2(pad * 2, textPanel.Height - pad);
        textPos.Y -= fullBlockHeight;

        spriteBatch.Begin();

        // Draw background, text, then redraw the background into the padding to cut any excess off the top
        Rectangle fullBg = new(0, 0, textPanel.Width, textPanel.Height);
        Rectangle padCover1 = new(0, 0, textPanel.Width, pad);
        Rectangle padCover2 = new(0, textPanel.Height - pad, textPanel.Width, pad);

        spriteBatch.Draw(basicQuad, fullBg, DarkBackgroundColor);
        msgFont.DrawText(spriteBatch, fullMessageText, textPos, Color.White, lineSpacing: msgLineSpacing);
        spriteBatch.Draw(basicQuad, padCover1, DarkBackgroundColor);
        spriteBatch.Draw(basicQuad, padCover2, DarkBackgroundColor);

        spriteBatch.End();
    }

    /// <summary>
    /// Redraws the main screen.
    /// </summary>
    ///
    /// <remarks>
    /// You do not need to call this method. It is called automatically every frame.
    /// </remarks>
    protected sealed override void Draw(GameTime gameTime)
    {
        // Update panel contents if necessary
        mainPanel.RedrawContents(gameTime);
        infoPanel.RedrawContents(gameTime);
        textPanel.RedrawContents(gameTime);

        // Redraw panel textures to the screen
        GraphicsDevice.Clear(MainBackgroundColor);

        spriteBatch.Begin(SpriteSortMode.Texture);
        spriteBatch.Draw(mainPanel.Texture, mainPanel.Destination, Color.White);
        spriteBatch.Draw(infoPanel.Texture, infoPanel.Destination, Color.White);
        spriteBatch.Draw(textPanel.Texture, textPanel.Destination, Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }


    #endregion
}
