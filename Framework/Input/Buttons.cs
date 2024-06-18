namespace TrentCOIS.Tools.Visualization.Input;

using Microsoft.Xna.Framework.Input;

/// <summary>
/// The mouse buttons that a user may press or release on a given frame.
/// </summary>
public enum MouseButton
{
    /// <summary>Left mouse button.</summary>
    Left,
    /// <summary>Right mouse button.</summary>
    Right,
    /// <summary>Middle mouse button/scroll-wheel.</summary>
    Middle,
    /// <summary>Mouse button four, usually the <i>back</i> button.</summary>
    Button4,
    /// <summary>Mouse button five, usually the <i>forward</i> button.</summary>
    Button5,
}

/*
 * - We want the MonoGame `Keys` enum, but we want it to be called `Key` instead.
 * - We want all of the keys except for the `None` key.
 *
 * C# has no way to re-export members from a dependency. It also has no way to "inherit" from or rename enums. Oh
 * well... Nothing a little bit of copy-paste can't help with!
 */

/// <summary>
/// The keyboard keys that a user may press or release on a given frame.
/// </summary>
/// <remarks>
/// This enum maps directly to MonoGame's <see cref="Keys"/> enum.
/// </remarks>
public enum Key
{
    // cspell:disable
    #pragma warning disable format
    /** <summary>BACKSPACE key.</summary> */ Backspace = Keys.Back,
    /** <summary>TAB key.</summary> */ Tab = Keys.Tab,
    /** <summary>ENTER key.</summary> */ Enter = Keys.Enter,
    /** <summary>CAPS LOCK key.</summary> */ CapsLock = Keys.CapsLock,
    /** <summary>ESC key.</summary> */ Escape = Keys.Escape,
    /** <summary>SPACEBAR key.</summary> */ Space = Keys.Space,
    // =================================================================================================================
    /** <summary>PAGE UP key.</summary> */ PageUp = Keys.PageUp,
    /** <summary>PAGE DOWN key.</summary> */ PageDown = Keys.PageDown,
    /** <summary>END key.</summary> */ End = Keys.End,
    /** <summary>HOME key.</summary> */ Home = Keys.Home,
    /** <summary>LEFT ARROW key.</summary> */ Left = Keys.Left,
    /** <summary>UP ARROW key.</summary> */ Up = Keys.Up,
    /** <summary>RIGHT ARROW key.</summary> */ Right = Keys.Right,
    /** <summary>DOWN ARROW key.</summary> */ Down = Keys.Down,
    // =================================================================================================================
    /** <summary>SELECT key.</summary> */ Select = Keys.Select,
    /** <summary>PRINT key.</summary> */ Print = Keys.Print,
    /** <summary>EXECUTE key.</summary> */ Execute = Keys.Execute,
    /** <summary>PRINT SCREEN key.</summary> */ PrintScreen = Keys.PrintScreen,
    /** <summary>INS key.</summary> */ Insert = Keys.Insert,
    /** <summary>DEL key.</summary> */ Delete = Keys.Delete,
    /** <summary>HELP key.</summary> */ Help = Keys.Help,
    // =================================================================================================================
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D0 = Keys.D0,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D1 = Keys.D1,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D2 = Keys.D2,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D3 = Keys.D3,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D4 = Keys.D4,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D5 = Keys.D5,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D6 = Keys.D6,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D7 = Keys.D7,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D8 = Keys.D8,
    /** <summary>Miscellaneous character; can vary by keyboard.</summary> */ D9 = Keys.D9,
    // =================================================================================================================
    /** <summary>A key.</summary> */ A = Keys.A,
    /** <summary>B key.</summary> */ B = Keys.B,
    /** <summary>C key.</summary> */ C = Keys.C,
    /** <summary>D key.</summary> */ D = Keys.D,
    /** <summary>E key.</summary> */ E = Keys.E,
    /** <summary>F key.</summary> */ F = Keys.F,
    /** <summary>G key.</summary> */ G = Keys.G,
    /** <summary>H key.</summary> */ H = Keys.H,
    /** <summary>I key.</summary> */ I = Keys.I,
    /** <summary>J key.</summary> */ J = Keys.J,
    /** <summary>K key.</summary> */ K = Keys.K,
    /** <summary>L key.</summary> */ L = Keys.L,
    /** <summary>M key.</summary> */ M = Keys.M,
    /** <summary>N key.</summary> */ N = Keys.N,
    /** <summary>O key.</summary> */ O = Keys.O,
    /** <summary>P key.</summary> */ P = Keys.P,
    /** <summary>Q key.</summary> */ Q = Keys.Q,
    /** <summary>R key.</summary> */ R = Keys.R,
    /** <summary>S key.</summary> */ S = Keys.S,
    /** <summary>T key.</summary> */ T = Keys.T,
    /** <summary>U key.</summary> */ U = Keys.U,
    /** <summary>V key.</summary> */ V = Keys.V,
    /** <summary>W key.</summary> */ W = Keys.W,
    /** <summary>X key.</summary> */ X = Keys.X,
    /** <summary>Y key.</summary> */ Y = Keys.Y,
    /** <summary>Z key.</summary> */ Z = Keys.Z,
    // =================================================================================================================
    /** <summary>Left Windows key.</summary> */ LeftWindows = Keys.LeftWindows,
    /** <summary>Right Windows key.</summary> */ RightWindows = Keys.RightWindows,
    /** <summary>Applications key.</summary> */ Apps = Keys.Apps,
    /** <summary>Computer Sleep key.</summary> */ Sleep = Keys.Sleep,
    // =================================================================================================================
    /** <summary>Numeric keypad 0 key.</summary> */ NumPad0 = Keys.NumPad0,
    /** <summary>Numeric keypad 1 key.</summary> */ NumPad1 = Keys.NumPad1,
    /** <summary>Numeric keypad 2 key.</summary> */ NumPad2 = Keys.NumPad2,
    /** <summary>Numeric keypad 3 key.</summary> */ NumPad3 = Keys.NumPad3,
    /** <summary>Numeric keypad 4 key.</summary> */ NumPad4 = Keys.NumPad4,
    /** <summary>Numeric keypad 5 key.</summary> */ NumPad5 = Keys.NumPad5,
    /** <summary>Numeric keypad 6 key.</summary> */ NumPad6 = Keys.NumPad6,
    /** <summary>Numeric keypad 7 key.</summary> */ NumPad7 = Keys.NumPad7,
    /** <summary>Numeric keypad 8 key.</summary> */ NumPad8 = Keys.NumPad8,
    /** <summary>Numeric keypad 9 key.</summary> */ NumPad9 = Keys.NumPad9,
    /** <summary>Multiply key.</summary> */ Multiply = Keys.Multiply,
    /** <summary>Add key.</summary> */ Add = Keys.Add,
    /** <summary>Separator key.</summary> */ Separator = Keys.Separator,
    /** <summary>Subtract key.</summary> */ Subtract = Keys.Subtract,
    /** <summary>Decimal key.</summary> */ Decimal = Keys.Decimal,
    /** <summary>Divide key.</summary> */ Divide = Keys.Divide,
    // =================================================================================================================
    /** <summary>F1 key.</summary> */ F1 = Keys.F1,
    /** <summary>F2 key.</summary> */ F2 = Keys.F2,
    /** <summary>F3 key.</summary> */ F3 = Keys.F3,
    /** <summary>F4 key.</summary> */ F4 = Keys.F4,
    /** <summary>F5 key.</summary> */ F5 = Keys.F5,
    /** <summary>F6 key.</summary> */ F6 = Keys.F6,
    /** <summary>F7 key.</summary> */ F7 = Keys.F7,
    /** <summary>F8 key.</summary> */ F8 = Keys.F8,
    /** <summary>F9 key.</summary> */ F9 = Keys.F9,
    /** <summary>F10 key.</summary> */ F10 = Keys.F10,
    /** <summary>F11 key.</summary> */ F11 = Keys.F11,
    /** <summary>F12 key.</summary> */ F12 = Keys.F12,
    /** <summary>F13 key.</summary> */ F13 = Keys.F13,
    /** <summary>F14 key.</summary> */ F14 = Keys.F14,
    /** <summary>F15 key.</summary> */ F15 = Keys.F15,
    /** <summary>F16 key.</summary> */ F16 = Keys.F16,
    /** <summary>F17 key.</summary> */ F17 = Keys.F17,
    /** <summary>F18 key.</summary> */ F18 = Keys.F18,
    /** <summary>F19 key.</summary> */ F19 = Keys.F19,
    /** <summary>F20 key.</summary> */ F20 = Keys.F20,
    /** <summary>F21 key.</summary> */ F21 = Keys.F21,
    /** <summary>F22 key.</summary> */ F22 = Keys.F22,
    /** <summary>F23 key.</summary> */ F23 = Keys.F23,
    /** <summary>F24 key.</summary> */ F24 = Keys.F24,
    // =================================================================================================================
    /** <summary>NUM LOCK key.</summary> */ NumLock = Keys.NumLock,
    /** <summary>SCROLL LOCK key.</summary> */ Scroll = Keys.Scroll,
    /** <summary>Left SHIFT key.</summary> */ LeftShift = Keys.LeftShift,
    /** <summary>Right SHIFT key.</summary> */ RightShift = Keys.RightShift,
    /** <summary>Left CONTROL key.</summary> */ LeftControl = Keys.LeftControl,
    /** <summary>Right CONTROL key.</summary> */ RightControl = Keys.RightControl,
    /** <summary>Left ALT key.</summary> */ LeftAlt = Keys.LeftAlt,
    /** <summary>Right ALT key.</summary> */ RightAlt = Keys.RightAlt,
    // =================================================================================================================
    /** <summary>Browser Back key.</summary> */ BrowserBack = Keys.BrowserBack,
    /** <summary>Browser Forward key.</summary> */ BrowserForward = Keys.BrowserForward,
    /** <summary>Browser Refresh key.</summary> */ BrowserRefresh = Keys.BrowserRefresh,
    /** <summary>Browser Stop key.</summary> */ BrowserStop = Keys.BrowserStop,
    /** <summary>Browser Search key.</summary> */ BrowserSearch = Keys.BrowserSearch,
    /** <summary>Browser Favorites key.</summary> */ BrowserFavorites = Keys.BrowserFavorites,
    /** <summary>Browser Start and Home key.</summary> */ BrowserHome = Keys.BrowserHome,
    /** <summary>Volume Mute key.</summary> */ VolumeMute = Keys.VolumeMute,
    /** <summary>Volume Down key.</summary> */ VolumeDown = Keys.VolumeDown,
    /** <summary>Volume Up key.</summary> */ VolumeUp = Keys.VolumeUp,
    /** <summary>Next Track key.</summary> */ MediaNextTrack = Keys.MediaNextTrack,
    /** <summary>Previous Track key.</summary> */ MediaPreviousTrack = Keys.MediaPreviousTrack,
    /** <summary>Stop Media key.</summary> */ MediaStop = Keys.MediaStop,
    /** <summary>Play/Pause Media key.</summary> */ MediaPlayPause = Keys.MediaPlayPause,
    /** <summary>Start Mail key.</summary> */ LaunchMail = Keys.LaunchMail,
    /** <summary>Select Media key.</summary> */ SelectMedia = Keys.SelectMedia,
    /** <summary>Start Application 1 key.</summary> */ LaunchApplication1 = Keys.LaunchApplication1,
    /** <summary>Start Application 2 key.</summary> */ LaunchApplication2 = Keys.LaunchApplication2,
    // =================================================================================================================
    /** <summary>The OEM Semicolon key on a US standard keyboard.</summary> */ OemSemicolon = Keys.OemSemicolon,
    /** <summary>For any country/region, the '+' key.</summary> */ OemPlus = Keys.OemPlus,
    /** <summary>For any country/region, the ',' key.</summary> */ OemComma = Keys.OemComma,
    /** <summary>For any country/region, the '-' key.</summary> */ OemMinus = Keys.OemMinus,
    /** <summary>For any country/region, the '.' key.</summary> */ OemPeriod = Keys.OemPeriod,
    /** <summary>The OEM question mark key on a US standard keyboard.</summary> */ OemQuestion = Keys.OemQuestion,
    /** <summary>The OEM tilde key on a US standard keyboard.</summary> */ OemTilde = Keys.OemTilde,
    /** <summary>The OEM open bracket key on a US standard keyboard.</summary> */ OemOpenBrackets = Keys.OemOpenBrackets,
    /** <summary>The OEM pipe key on a US standard keyboard.</summary> */ OemPipe = Keys.OemPipe,
    /** <summary>The OEM close bracket key on a US standard keyboard.</summary> */ OemCloseBrackets = Keys.OemCloseBrackets,
    /** <summary>The OEM singled/double quote key on a US standard keyboard.</summary> */ OemQuotes = Keys.OemQuotes,
    /** <summary>Used for miscellaneous characters; it can vary by keyboard.</summary> */ Oem8 = Keys.Oem8,
    /** <summary>The OEM angle bracket or backslash key on the RT 102 key keyboard.</summary> */ OemBackslash = Keys.OemBackslash,
    /** <summary>IME PROCESS key.</summary> */ ProcessKey = Keys.ProcessKey,
    /** <summary>Attn key.</summary> */ Attn = Keys.Attn,
    /** <summary>CrSel key.</summary> */ Crsel = Keys.Crsel,
    /** <summary>ExSel key.</summary> */ Exsel = Keys.Exsel,
    /** <summary>Erase EOF key.</summary> */ EraseEof = Keys.EraseEof,
    /** <summary>Play key.</summary> */ Play = Keys.Play,
    /** <summary>Zoom key.</summary> */ Zoom = Keys.Zoom,
    /** <summary>PA1 key.</summary> */ Pa1 = Keys.Pa1,
    /** <summary>CLEAR key.</summary> */ OemClear = Keys.OemClear,
    /** <summary>Green ChatPad key.</summary> */ ChatPadGreen = Keys.ChatPadGreen,
    /** <summary>Orange ChatPad key.</summary> */ ChatPadOrange = Keys.ChatPadOrange,
    /** <summary>PAUSE key.</summary> */ Pause = Keys.Pause,
    /** <summary>IME Convert key.</summary> */ ImeConvert = Keys.ImeConvert,
    /** <summary>IME NoConvert key.</summary> */ ImeNoConvert = Keys.ImeNoConvert,
    /** <summary>Kana key on Japanese keyboards.</summary> */ Kana = Keys.Kana,
    /** <summary>Kanji key on Japanese keyboards.</summary> */ Kanji = Keys.Kanji,
    /** <summary>OEM Auto key.</summary> */ OemAuto = Keys.OemAuto,
    /** <summary>OEM Copy key.</summary> */ OemCopy = Keys.OemCopy,
    /** <summary>OEM Enlarge Window key.</summary> */ OemEnlW = Keys.OemEnlW
    #pragma warning restore format
    // cspell:enable
}
