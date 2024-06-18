namespace TrentCOIS.Tools.Visualization.Assets;

using System;

/// <summary>
/// The exception that is thrown when an asset fails to load.
/// </summary>
/// <seealso cref="AssetLoader"/>
public class AssetLoadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetLoadException"/> class.
    /// </summary>
    public AssetLoadException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetLoadException"/> class with the specified error message.
    /// </summary>
    public AssetLoadException(string? message) : base(message)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetLoadException"/> class with a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    public AssetLoadException(string? message, Exception? innerException) : base(message, innerException)
    { }
}
