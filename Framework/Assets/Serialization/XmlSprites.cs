namespace TrentCOIS.Tools.Visualization.Assets.Serialization;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;


/// <summary>
/// The common base class for sprite-related information. If these properties appear on a <see cref="XmlSpriteGroup"/>,
/// they are inherited to any <see cref="XmlSprite"/> children that do not provide their own value.
/// </summary>
/// <seealso cref="XmlSpriteGroup"/>
/// <seealso cref="XmlSprite"/>
public record XmlSpriteInfo
{
    /// <summary>The X-coordinate of where this sprite lives within its texture atlas.</summary>
    [XmlIgnore] public ushort? X = null;

    /// <summary>The Y-coordinate of where this sprite lives within its texture atlas.</summary>
    [XmlIgnore] public ushort? Y = null;

    /// <summary>The width of this sprite within its source texture atlas.</summary>
    [XmlIgnore] public ushort? W = null;

    /// <summary>The height of this sprite within its source texture atlas.</summary>
    [XmlIgnore] public ushort? H = null;

    // XmlSerializer can't deserialize a nullable value type, but we still wanna be able to determine the difference
    // between null and zero. So we have this extra bit of indirection here.

    /// <summary>Extra field for deserialization.</summary>
    [XmlAttribute("X")] public ushort XAttr { get => X ?? 0; set => X = value; }
    /// <summary>Extra field for deserialization.</summary>
    [XmlAttribute("Y")] public ushort YAttr { get => Y ?? 0; set => Y = value; }
    /// <summary>Extra field for deserialization.</summary>
    [XmlAttribute("W")] public ushort WAttr { get => W ?? 0; set => W = value; }
    /// <summary>Extra field for deserialization.</summary>
    [XmlAttribute("H")] public ushort HAttr { get => H ?? 0; set => H = value; }


    /// <summary>The relative path to the texture file that this sprite or group of sprites come from.</summary>
    [XmlAttribute] public string? FilePath = null;


    /// <remarks>
    /// This constructor does nothing; properties are left uninitialized. This constructor needs to exist and be public
    /// so that the <see cref="XmlSerializer"/> can construct an instance to assign properties to.
    /// </remarks>
    /// <seealso cref="AssetLoader"/>
    /// <seealso href="https://stackoverflow.com/a/267727/10549827"/>
    public XmlSpriteInfo()
    { }


    /// <summary>
    /// Creates a new set of sprite info by replacing this instance's properties with any of the properties from
    /// <paramref name="other"/> that are not null.
    /// </summary>
    public XmlSpriteInfo With(XmlSpriteInfo other)
    {
        return this with
        {
            X = other.X ?? X,
            Y = other.Y ?? Y,
            W = other.W ?? W,
            H = other.H ?? H,
            FilePath = other.FilePath ?? FilePath,
        };
    }
}


/// <summary>
/// An XML element that contains information about a sprite's
/// </summary>
/// <remarks></remarks>
/// <seealso cref="AssetLoader"/>
[Serializable]
public record XmlSprite : XmlSpriteInfo
{
    /// <summary>This sprite's unique identifying name.</summary>
    [XmlAttribute] public string? Name = null;

    /// <summary>
    /// Initializes an empty <see cref="XmlSprite"/>.
    /// </summary>
    /// <inheritdoc path="/remarks"/>
    public XmlSprite()
    { }
}



/// <summary>
/// An XML element that contains nested groups of <see cref="XmlSprite"/>.
/// </summary>
/// <remarks>
/// When appearing as the root, this XML element is called <c>SpriteList</c>. Otherwise, it is called
/// <c>SpriteGroup</c>.
/// </remarks>
/// <seealso cref="AssetLoader"/>
[Serializable]
[XmlRoot("SpriteList")]
public record XmlSpriteGroup : XmlSpriteInfo
{
    /// <summary>
    /// Whether or not this is the root &lt;SpriteList&gt; of its file.
    /// </summary>
    [XmlIgnore] public bool IsRoot { get; private set; } = false;

    /// <summary>
    /// The name of this group of sprites.
    /// </summary>
    [XmlAttribute] public string? Name = null;

    /// <summary>
    /// All of the sprites that are directly inside of this group.
    /// </summary>
    [XmlElement("Sprite")]
    public List<XmlSprite> Sprites = new();

    /// <summary>
    /// All of the nested subgroups of this group.
    /// </summary>
    [XmlElement("SpriteGroup")]
    public List<XmlSpriteGroup> Children = new();


    /// <summary>
    /// Initializes an empty <see cref="XmlSpriteGroup"/>.
    /// </summary>
    /// <inheritdoc path="/remarks"/>
    public XmlSpriteGroup()
    { }


    /// <summary>
    /// Reads and deserializes an <see cref="XmlSpriteGroup"/> from an XML file.
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    /// <exception cref="AssetLoadException"></exception>
    public static XmlSpriteGroup LoadFromFile(string fullPath)
    {
        using var stream = AssetLoader.OpenFile(fullPath);
        var serializer = new XmlSerializer(typeof(XmlSpriteGroup));

        if (serializer.Deserialize(stream) is XmlSpriteGroup parsed)
        {
            parsed.IsRoot = true;
            return parsed;
        }
        else
        {
            throw new AssetLoadException($"Failed to parse Sprite XML data in file `{fullPath}`.");
        }
    }


    /// <summary>
    /// Counts how many sprites are within this group and all sub-groups.
    /// </summary>
    /// <returns>The total number of sprites within this entire group.</returns>
    public int CountTotal()
    {
        var stack = new Stack<XmlSpriteGroup>();
        stack.Push(this);

        int total = 0;
        while (stack.TryPop(out var group))
        {
            total += group.Sprites.Count;
            foreach (var child in group.Children) stack.Push(child);
        }

        return total;
    }
}
