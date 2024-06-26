namespace TrentCOIS.Tools.Visualization.Assets;

using System;
using System.IO;
using System.Security;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using TrentCOIS.Tools.Visualization.Assets.Serialization;


/// <summary>
/// Handles loading supported assets from files or streams.
/// </summary>
public class AssetLoader
{
    /// <summary>
    /// The graphics device used to load texture-related data.s
    /// </summary>
    protected GraphicsDevice GraphicsDevice { get; init; }

    /// <summary>
    /// A cache of already-loaded texture files, indexed by the full path of the file.
    /// </summary>
    protected Dictionary<string, Texture2D> TexFileCache { get; private set; }

    /// <summary>
    /// A cache of already-loaded font-file data.
    /// </summary>
    /// <remarks>
    /// Unfortunately, <see href="https://github.com/FontStashSharp/FontStashSharp/tree/1.3.7">FontStashSharp</see> does
    /// not provide any way to cache the results of an already-added font if you want to have multiple instances of
    /// <see cref="FontSystem"/> that use the same font.
    /// </remarks>
    protected Dictionary<string, byte[]> FontFileCache { get; private set; }


    /// <summary>
    /// The currently executing assembly. May be useful to have for obtaining resource streams or path.
    /// </summary>
    protected Assembly ExecAssembly { get; }


    /// <summary>
    /// Any provided and encountered asset paths are interpreted relative to this path.
    /// </summary>
    /// <value>
    /// The absolute system-path used as the relative starting point for all path-related operations.
    /// </value>
    /// <remarks>
    /// By default, this path is the directory of the currently executing assembly. NB: not <i>this library's</i>
    /// assembly's path, but the <b>currently executing</b> assembly's path.
    /// </remarks>
    public string AssetBasePath { get; set; }

    /// <summary>
    /// Sprite-sheet information is stored in XML files. When parsing this information, sprites' group-names are
    /// flattened into single strings. This is the character used to separate these groups.
    /// </summary>
    /// <value>
    /// The string this loader will use to separate sprite group names from XML when flattening the names into one
    /// string.
    /// </value>
    /// <seealso cref="XmlSpriteGroup"/>
    /// <seealso cref="XmlSprite"/>
    public string SpriteNameGroupSeparator { get; set; } = "/";


    /// <summary>
    /// Initializes a new AssetLoader.
    /// </summary>
    /// <param name="graphicsDevice"></param>
    public AssetLoader(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;

        TexFileCache = new();
        FontFileCache = new();

        ExecAssembly = Assembly.GetExecutingAssembly();
        AssetBasePath = Path.GetDirectoryName(ExecAssembly.Location)!;
        // NB: non-null since we know the assembly path is guaranteed to be an actual file.
        // NB: `Assembly.Location` docs state that it is fully qualified already.
    }


    #region Font loading

    /// <inheritdoc cref="LoadFont(IEnumerable{string})"/>
    /// <exception cref="ArgumentException">If no parameters are provided.</exception>
    public FontSystem LoadFont(params string[] fontFiles)
    {
        if (fontFiles.Length == 0)
        {
            var msg = $"{nameof(LoadFont)} requires at least one font file.";
            throw new ArgumentException(msg, nameof(fontFiles));
        }

        return LoadFont((IEnumerable<string>)fontFiles);
    }


    /// <summary>
    /// Creates a new <see cref="FontSystem"/>, which is a collection of one or more loaded files. A FontSystem can hold
    /// multiple fonts: each FontSystem represents a single "font-stack" with fallback fonts.
    /// </summary>
    /// <param name="fontFiles">
    /// One or more file paths pointing to font files, relative to <see cref="AssetBasePath"/>.
    /// </param>
    /// <returns>A new "font-stack."</returns>
    /// <exception cref="AssetLoadException">
    /// If any of the provided files could not be read or parsed as a font.
    /// </exception>
    public FontSystem LoadFont(IEnumerable<string> fontFiles)
    {
        var fontSystem = new FontSystem(new FontSystemSettings()
        {
            // We use 4x the default atlas size/resolution for higher fidelity.
            FontResolutionFactor = 4.0f,
            KernelWidth = 4,
            KernelHeight = 4,
            TextureWidth = FontSystemDefaults.TextureWidth * 2, // 2x on 2 sides = 4x increase in area
            TextureHeight = FontSystemDefaults.TextureHeight * 2,
        });

        foreach (string relPath in fontFiles)
        {
            string fullPath = Path.GetFullPath(relPath, AssetBasePath);
            if (FontFileCache.TryGetValue(fullPath, out byte[]? fontData))
            {
                // No need to try-catch here since it came from our cache of already-valid results
                fontSystem.AddFont(fontData);
            }
            else
            {
                fontData = ReadFile(fullPath);

                try
                {
                    fontSystem.AddFont(fontData);
                }
                catch (Exception err)
                {
                    // [FIXME] FontStashSharp does not use any custom exception types, just `Exception`. So we have no
                    // choice but to catch-all for this single line. If that ever changes, update this catch.

                    var fileName = Path.GetFileName(fullPath)!;
                    var exception = new AssetLoadException($"Failed to load font data from `{fullPath}`.", err);
                    exception.Data.Add("Base path for assets", AssetBasePath);
                    exception.Data.Add("Specified path", relPath);
                    exception.Data.Add("Resolved path", fullPath);
                    throw exception;
                }

                // After success, cache the results.
                FontFileCache.Add(fullPath, fontData);
            }
        }

        return fontSystem;
    }

    #endregion


    #region Sprite loading

    /// <summary>
    /// Gets a loaded texture image from <see cref="TexFileCache">cache</see> or loads it from disk.
    /// </summary>
    /// <param name="relPath">The image's path relative to <see cref="AssetBasePath"/>.</param>
    protected Texture2D GetTexture(string relPath) => GetTexture(relPath, AssetBasePath);

    /// <summary>
    /// <inheritdoc cref="GetTexture(string)" path="/summary"/>
    /// </summary>
    /// <param name="relPath">The image's path relative to <paramref name="basePath"/>.</param>
    /// <param name="basePath">The path from which relative paths should be resolved.</param>
    protected Texture2D GetTexture(string relPath, string basePath)
    {
        var texPath = Path.GetFullPath(relPath, basePath);
        var texture = TexFileCache.GetValueOrDefault(texPath);
        if (texture is null)
        {
            using Stream stream = OpenFile(texPath);
            texture = Texture2D.FromStream(GraphicsDevice, stream);
            TexFileCache.Add(texPath, texture);
        }

        return texture;
    }


    /// <summary>
    /// Loads a single sprite from a single image file.
    /// </summary>
    /// <param name="name">What to name the sprite.</param>
    /// <param name="imagePath"> A path relative to an image file, relative to <see cref="AssetBasePath"/>.</param>
    /// <returns>A single sprite whose <see cref="Sprite.SourceBounds"/> will take up the entire image.</returns>
    public Sprite LoadSpriteFromImage(string name, string imagePath)
    {
        var tex = GetTexture(imagePath); // handles caching
        return new Sprite(name, tex);
    }


    /// <inheritdoc cref="LoadSpriteGroups(IEnumerable{string})" />
    /// <exception cref="ArgumentException">If no parameters are provided.</exception>
    public Dictionary<string, Sprite> LoadSpriteGroups(params string[] xmlFiles)
    {
        if (xmlFiles.Length == 0)
        {
            var msg = $"{nameof(LoadSpriteGroups)} requires at least one XML file.";
            throw new ArgumentException(msg, nameof(xmlFiles));
        }

        return LoadSpriteGroups((IEnumerable<string>)xmlFiles);
    }


    /// <summary>
    /// Loads a group of sprites from an XML file.
    /// </summary>
    /// <param name="xmlFiles">
    /// One or more file paths pointing to <c>&lt;SpriteList&gt;</c> XML files. Paths should be relative to
    /// <see cref="AssetBasePath"/>.
    /// </param>
    /// <seealso cref="XmlSpriteGroup"/>
    /// <seealso cref="XmlSprite"/>
    /// <returns>
    /// A dictionary of all sprites loaded from the XML file. Keys are the sprites' names, prefixed with their group
    /// names. Group names are separated by <see cref="SpriteNameGroupSeparator"/>.
    /// </returns>
    /// <remarks>
    /// Duplicated sprite names will result in an error. To parse multiple XML files into multiple distinct sets, call
    /// this method more than once.
    /// </remarks>
    /// <exception cref="AssetLoadException">
    /// <para>If any invalid XML is found or if one of the files cannot be opened/read.</para>
    /// -or-
    /// <para>If two or more sprites share a fully-qualified name (after adding groups).</para>
    /// </exception>
    public Dictionary<string, Sprite> LoadSpriteGroups(IEnumerable<string> xmlFiles)
    {
        var allSprites = new Dictionary<string, Sprite>();

        foreach (string relPath in xmlFiles)
        {
            var xmlPath = Path.GetFullPath(relPath, AssetBasePath);
            try
            {
                var xmlRoot = XmlSpriteGroup.LoadFromFile(fullPath: xmlPath);

                int spriteCount = xmlRoot.CountTotal();
                allSprites.EnsureCapacity(allSprites.Count + spriteCount);

                var xmlInfo = new XmlSpriteInfo();
                var dirPath = Path.GetDirectoryName(xmlPath)!; // non-null since we know it's a valid file
                ParseXmlSpriteGroup(xmlRoot, dirPath, "", xmlInfo, ref allSprites);
            }
            catch (AssetLoadException inner)
            {
                // New exception so we can add path/filename information.
                var fileName = Path.GetFileName(xmlPath)!;
                var exception = new AssetLoadException($"Failed to load sprite data from `{xmlPath}`.", inner);
                exception.Data.Add("Base path for assets", AssetBasePath);
                exception.Data.Add("Specified path", relPath);
                exception.Data.Add("Resolved path", xmlPath);
                throw exception;
            }
        }

        return allSprites;
    }


    #region XML parsing

    /// <summary>
    /// Recursively parses a tree of sprite information.
    /// </summary>
    /// <param name="group">The group to parse.</param>
    /// <param name="xmlBasePath">
    /// The path to the directory containing the original XML file (so it can be joined with paths from within the
    /// file).
    /// </param>
    /// <param name="currentGroupName">
    /// The name of the current parent group, with its trailing slash (unless this is the root node, then it should be
    /// empty).
    /// </param>
    /// <param name="spriteInfo">Common sprite information that is merged together while descending the tree.</param>
    /// <param name="loadedSprites">The dictionary to load sprites into.</param>
    /// <exception cref="AssetLoadException">
    /// <para>If any invalid XML is found or if one of the files cannot be opened/read.</para>
    /// -or-
    /// <para>If two or more sprites share a fully-qualified name (after adding groups).</para>
    /// </exception>
    private void ParseXmlSpriteGroup(
        XmlSpriteGroup group,
        string xmlBasePath,
        string currentGroupName,
        XmlSpriteInfo spriteInfo,
        ref Dictionary<string, Sprite> loadedSprites
    )
    {
        // Merge this new group's information onto our "stack" of sprite properties
        spriteInfo = spriteInfo.With(group);

        // The group name needs to be done separately because it's an append instead of an override
        if (group.Name is not null) currentGroupName += group.Name + SpriteNameGroupSeparator;
        else if (!group.IsRoot) throw new AssetLoadException("Encountered a <SpriteGroup> without a Name attribute.");

        // Parse the direct child sprites before the child groups
        foreach (var xmlSprite in group.Sprites)
        {
            // The final properties used for a sprite come from the combination of all of its parent
            var finalInfo = spriteInfo.With(xmlSprite);

            var spriteTex = (finalInfo.FilePath is string texPath)
                ? GetTexture(texPath, xmlBasePath)
                : throw new AssetLoadException("Encountered a <Sprite> with no ancestor FilePath attribute.");

            if (xmlSprite.Name is null)
                throw new AssetLoadException("Encountered a <Sprite> without a Name attribute.");

            // Any sprite without (X, Y, W, or H) attributes just fallback to the texture size.
            var bounds = new Rectangle(
                finalInfo.X ?? 0,
                finalInfo.Y ?? 0,
                finalInfo.W ?? spriteTex.Width,
                finalInfo.H ?? spriteTex.Height
            );

            var key = currentGroupName + xmlSprite.Name;
            var sprite = new Sprite(xmlSprite.Name, spriteTex, bounds);
            if (!loadedSprites.TryAdd(key, sprite))
            {
                throw new AssetLoadException($"Encountered a duplicate <Sprite> name: {key}.");
            }
        }

        // Now recurse with the child groups.
        foreach (var xmlGroup in group.Children)
        {
            ParseXmlSpriteGroup(xmlGroup, xmlBasePath, currentGroupName, spriteInfo, ref loadedSprites);
        }
    }

    #endregion

    #endregion


    #region Exception helpers

    /// <summary>
    /// Opens a filepath as a stream and handles the re-throwing of exceptions.
    /// </summary>
    /// <param name="fullPath">The file path to open.</param>
    /// <returns>
    /// The result of <see cref="File.Open(string, FileMode)"/> with a <c>mode</c> of <see cref="FileMode.Open"/>.
    /// </returns>
    /// <exception cref="AssetLoadException">If anything goes wrong opening the file.</exception>
    protected internal static FileStream OpenFile(string fullPath)
    {
        try
        {
            return File.Open(fullPath, FileMode.Open);
        }
        catch (SystemException ex)
        {
            switch (ex)
            {
                // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.open?view=net-6.0#system-io-file-open(system-string-system-io-filemode)
                case FileNotFoundException:
                case DirectoryNotFoundException:
                    throw new AssetLoadException($"Could not find file.", ex);
                case PathTooLongException:  // "The specified path, file name, or both exceed the system-defined maximum length."
                case NotSupportedException: // "path is in an invalid format."
                    throw new AssetLoadException($"The specified path is invalid.", ex);
                case UnauthorizedAccessException:
                    throw new AssetLoadException($"File access is not authorized.`", ex);
                case IOException:
                    throw new AssetLoadException($"Something went wrong opening the file.", ex);
                // Anything else is on us.
                default: throw;
            }
        }
    }


    /// <summary>
    /// Reads all the bytes of a file and handles the re-throwing of exceptions.
    /// </summary>
    /// <param name="fullPath">The absolute path to the file.</param>
    /// <returns>All of the bytes contained in the file.</returns>
    /// <exception cref="AssetLoadException">If anything goes wrong opening the file.</exception>
    protected internal static byte[] ReadFile(string fullPath)
    {
        try
        {
            return File.ReadAllBytes(fullPath);
        }
        catch (SystemException ex)
        {
            switch (ex)
            {
                // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.readallbytes?view=net-6.0
                case FileNotFoundException:
                case DirectoryNotFoundException:
                    throw new AssetLoadException($"Could not find file.", ex);
                case PathTooLongException:  // "The specified path, file name, or both exceed the system-defined maximum length."
                case NotSupportedException: // "path is in an invalid format."
                    throw new AssetLoadException($"The specified path is invalid.", ex);
                case SecurityException:
                case UnauthorizedAccessException:
                    throw new AssetLoadException($"File access is not authorized.`", ex);
                case IOException:
                    throw new AssetLoadException($"Something went wrong opening the file.", ex);
                // Anything else is on us.
                default: throw;
            }
        }
    }

    #endregion
}
