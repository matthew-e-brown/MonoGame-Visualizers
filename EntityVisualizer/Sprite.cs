using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrentCOIS.Tools.Visualization.EntityViz
{
    using SpriteSerialization;

    public class Sprite
    {
        public readonly string Name;
        public readonly Texture2D Texture;
        public Rectangle SourceRect;

        public int Width { get => SourceRect.Width; set => SourceRect.Width = value; }
        public int Height { get => SourceRect.Height; set => SourceRect.Height = value; }

        /// <summary>
        /// Instantiation is done by the static <c cref="LoadAll(GraphicsDevice)" method. />
        /// </summary>
        private Sprite(string name, Texture2D texture, Rectangle location)
        {
            Name = name;
            Texture = texture;
            SourceRect = location;
        }

        // ------------------------------------------------------------------------

        /// <summary>
        /// Returns a new version of this sprite with a smaller source rectangle.
        /// </summary>
        public Sprite Cropped(Rectangle innerSourceRect)
        {
            // If the current sprite lives at (100, 200), and they want to crop to (2, 5, 2, 2), then they want (102,
            // 205, 2, 2).
            int texX = SourceRect.X + innerSourceRect.X;
            int texY = SourceRect.Y + innerSourceRect.Y;
            int newW = Math.Clamp(innerSourceRect.Width, 0, SourceRect.Width);
            int newH = Math.Clamp(innerSourceRect.Height, 0, SourceRect.Height);

            Rectangle newSrc = new(texX, texY, newW, newH);
            return new Sprite(Name, Texture, newSrc);
        }

        #region Drawing + overloads

        /// <summary>
        /// Draws this sprite centered at a given point.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="position">Where to position this sprite.</param>
        public void DrawCentered(SpriteBatch spriteBatch, Vector2 pos) => DrawCentered(spriteBatch, pos, Color.White);

        /// <summary>
        /// Draws this sprite centered at a given point.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="position">Where to position this sprite.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void DrawCentered(SpriteBatch spriteBatch, Vector2 pos, Color tint) => DrawCentered(spriteBatch, (int)pos.X, (int)pos.Y, Width, Height, tint);


        /// <summary>
        /// Draws this sprite centered at a given point and at a given size.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="position">Where to position this sprite.</param>
        public void DrawCentered(SpriteBatch spriteBatch, Vector2 pos, int w, int h) => DrawCentered(spriteBatch, pos, w, h, Color.White);

        /// <summary>
        /// Draws this sprite centered at a given point and at a given size.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="position">Where to position this sprite.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void DrawCentered(SpriteBatch spriteBatch, Vector2 pos, int w, int h, Color tint) => DrawCentered(spriteBatch, (int)pos.X, (int)pos.Y, w, h, tint);


        /// <summary>
        /// Draws this sprite centered at a given point.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        public void DrawCentered(SpriteBatch spriteBatch, int x, int y) => DrawCentered(spriteBatch, x, y, Width, Height, Color.White);

        /// <summary>
        /// Draws this sprite centered at a given point.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void DrawCentered(SpriteBatch spriteBatch, int x, int y, Color tint) => DrawCentered(spriteBatch, x, y, Width, Height, tint);


        /// <summary>
        /// Draws this sprite centered at a given point and at a given size.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        public void DrawCentered(SpriteBatch spriteBatch, int x, int y, int w, int h) => DrawCentered(spriteBatch, x, y, w, h, Color.White);

        /// <summary>
        /// Draws this sprite centered at a given point and at a given size.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void DrawCentered(SpriteBatch spriteBatch, int x, int y, int w, int h, Color tint)
        {
            x -= w / 2;
            y -= h / 2;
            Draw(spriteBatch, x, y, w, h, tint);
        }


        /// <summary>
        /// Draws this sprite with its top-left corner at a given X and Y position.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        public void Draw(SpriteBatch spriteBatch, int x, int y) => Draw(spriteBatch, x, y, Width, Height);

        /// <summary>
        /// Draws this sprite with its top-left corner at a given X and Y position.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void Draw(SpriteBatch spriteBatch, int x, int y, Color tint) => Draw(spriteBatch, x, y, Width, Height, tint);


        /// <summary>
        /// Draws this sprite with its top-left corner at a given X and Y position and at a given size.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        public void Draw(SpriteBatch spriteBatch, int x, int y, int w, int h) => Draw(spriteBatch, new Rectangle(x, y, w, h));

        public void Draw(SpriteBatch spriteBatch, int x, int y, int w, int h, Color tint) => Draw(spriteBatch, new Rectangle(x, y, w, h), tint);


        /// <summary>
        /// Draws this sprite, filling the given destination rectangle.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="dest">The destination location.</param>
        /// <param name="tint">A color to tint the sprite with.</param>
        public void Draw(SpriteBatch spriteBatch, Rectangle dest) => Draw(spriteBatch, dest, Color.White);

        /// <summary>
        /// Draws this sprite, filling the given destination rectangle.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw into.</param>
        /// <param name="dest">The destination location.</param>
        public void Draw(SpriteBatch spriteBatch, Rectangle dest, Color tint)
        {
            spriteBatch.Draw(Texture, dest, SourceRect, tint);
        }


        #endregion

        // ------------------------------------------------------------------------

        #region XML Parsing

        public static readonly string SpriteResourcePath;
        public static readonly string SpriteGroupSeparator = "/"; // Groups = slash, sprite variants = dot

        static Sprite()
        {
            // Sprites should have been copied to the output folder, and this assembly should be running from there.
            string assemblyDir = Path.GetDirectoryName(typeof(Sprite).Assembly.Location)!;
            string spriteDir = Path.Join(assemblyDir, "Resources/Sprites");
            SpriteResourcePath = Path.GetFullPath(spriteDir);
        }


        public static Dictionary<string, Sprite> LoadAll(GraphicsDevice graphicsDevice)
        {
            var xmlFiles = Directory.GetFiles(SpriteResourcePath, "*.xml");
            var allSprites = new Dictionary<string, Sprite>();
            var textureCache = new Dictionary<string, Texture2D>();

            foreach (string path in xmlFiles)
            {
                ParseXmlFile(graphicsDevice, path, ref allSprites, ref textureCache);
            }

            return allSprites;
        }


        private static void ParseXmlFile(
            GraphicsDevice graphicsDevice,
            string xmlPath,
            ref Dictionary<string, Sprite> loadedSprites,
            ref Dictionary<string, Texture2D> textureCache
        )
        {
            XmlSpriteGroup xmlRoot;
            using (var stream = File.Open(xmlPath, FileMode.Open))
            {
                if (stream is null)
                    throw new Exception("Failed to open resource stream for SpriteMap.xml.");

                var xml = new XmlSerializer(typeof(XmlSpriteGroup));
                if (xml.Deserialize(stream) is XmlSpriteGroup parsed)
                    xmlRoot = parsed;
                else
                    throw new InvalidDataException("Failed to parse SpiteMap.xml.");
            }

            ParseXmlGroup(graphicsDevice, xmlRoot, ref loadedSprites, ref textureCache);
        }

        /// <summary>
        /// Recursively parses a group of sprite data from XML starting from the root node.
        /// </summary>
        private static void ParseXmlGroup(
            GraphicsDevice gfx,
            XmlSpriteGroup group,
            ref Dictionary<string, Sprite> loadedSprites,
            ref Dictionary<string, Texture2D> textureCache
        )
        {
            var startFile = (group.FilePath is string path) ? GetTexture(gfx, path, ref textureCache) : null;
            ParseXmlGroup(gfx, group, "", startFile, ref loadedSprites, ref textureCache, true);
        }

        /// <summary>
        /// Recursively parses a group of sprite data from XML.
        /// </summary>
        private static void ParseXmlGroup(
            GraphicsDevice gfx,
            XmlSpriteGroup group,
            string groupName,       // Prepended directly onto sprite name (should include trailing slash)
            Texture2D? currTexture,
            ref Dictionary<string, Sprite> loadedSprites,
            ref Dictionary<string, Texture2D> textureCache,
            bool isRootNode = false
        )
        {
            if (!isRootNode && group.Name is null)
                throw new InvalidDataException("Encountered non-root <SpriteGroup> without a name.");

            // All sprites and child-groups in this group should have this group as a prefix, but only if this group
            // actually has one.
            if (group.Name is not null)
                groupName += group.Name + SpriteGroupSeparator;

            var sprites = group.Sprites;
            var childGroups = group.Groups;

            // If this group has a file path, we need to open the new texture
            if (group.FilePath is string newPath)
                currTexture = GetTexture(gfx, newPath, ref textureCache);

            // Handle all the groups within this group.
            foreach (var child in childGroups)
                ParseXmlGroup(gfx, child, groupName, currTexture, ref loadedSprites, ref textureCache);

            // Then handle all the sprites within this group
            if (sprites.Count > 0)
            {
                // If this group has any sprites, but no level so far as given a filepath, we have a problem!
                if (currTexture is null)
                    throw new InvalidDataException("Encountered <Sprite> with no ancestor FilePath attribute.");

                // Grab all the XML data and create proper sprite objects referencing the texture
                loadedSprites.EnsureCapacity(loadedSprites.Count + sprites.Count);
                foreach (var xmlSprite in sprites)
                {
                    var rect = new Rectangle(xmlSprite.X, xmlSprite.Y, xmlSprite.W, xmlSprite.H);
                    var loaded = new Sprite(xmlSprite.Name, currTexture, rect);
                    var fullName = groupName + xmlSprite.Name;
                    loadedSprites.Add(fullName, loaded);
                }
            }
        }

        /// <summary>
        /// Gets a loaded texture from cache or loads it from disk.
        /// </summary>
        private static Texture2D GetTexture(
            GraphicsDevice gfx,
            string filePath,
            ref Dictionary<string, Texture2D> cache
        )
        {
            var texPath = Path.GetFullPath(Path.Join(SpriteResourcePath, filePath));
            var texture = cache.GetValueOrDefault(texPath);
            if (texture is null)
            {
                using Stream stream = File.Open(texPath, FileMode.Open);
                texture = Texture2D.FromStream(gfx, stream);
                cache.Add(texPath, texture);
            }

            return texture;
        }

        #endregion
    }

    #region XML Serializing

    namespace SpriteSerialization
    {
        [Serializable]
        [XmlRoot("SpriteList")] // When root, appears as "SpriteList" instead of "SpriteGroup".
        public record XmlSpriteGroup
        {
            [XmlAttribute] public string? Name = null;
            [XmlAttribute] public string? FilePath = null;

            [XmlElement("Sprite")]
            public List<XmlSprite> Sprites = new();

            [XmlElement("SpriteGroup")] // Can contain nested copies of itself.
            public List<XmlSpriteGroup> Groups = new();

            private XmlSpriteGroup() { } // https://stackoverflow.com/a/267727/10549827
        }

        [Serializable]
        public record struct XmlSprite
        {
            [XmlAttribute] public string Name;
            [XmlAttribute] public ushort X;
            [XmlAttribute] public ushort Y;
            [XmlAttribute] public ushort W;
            [XmlAttribute] public ushort H;

            public XmlSprite()
            {
                Name = "";
                X = Y = 0;
                W = H = 10;
            }
        }
    }

    #endregion
}
