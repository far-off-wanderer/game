namespace Far_Off_Wanderer;

using Far_Off_Wanderer.Tools;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Content
{
    readonly ContentManager contentManager;
    readonly GraphicsDevice graphicsDevice;
    readonly string root;

    public Content(ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        this.contentManager = contentManager;
        this.graphicsDevice = graphicsDevice;
        this.root = contentManager.RootDirectory;
    }

    private Content(Content content, string subdirectory)
    {
        this.contentManager = content.contentManager;
        this.graphicsDevice = content.graphicsDevice;
        this.root = Path.Combine(content.root, subdirectory);
    }

    public Content For(string scene) => new(this, Path.Combine("Scenes", scene));

    T Load<T>(string name, Func<Stream, T> loader, params string[] extensions)
    {
        var searchPath = Path.Combine(root, name + ".*");

        var pathToVerify = Path.GetDirectoryName(searchPath);

        if (Directory.Exists(pathToVerify))
        {
            var file = Directory.EnumerateFiles(root, $"{name}.*").Where(f => extensions.Contains(Path.GetExtension(f))).FirstOrDefault();
            if (file != null)
            {
                return loader(File.OpenRead(file));
            }
        }
        return contentManager.Load<T>(name);
    }

    Texture2D GetTexture(string textureName) => Load(textureName, stream => Texture2D.FromStream(graphicsDevice, stream), ".png", ".jpg");
    Image<Rgba32> GetImage(string imageName) => Load(imageName, stream => Image.Load<Rgba32>(stream), ".png", ".jpg", ".bmp", ".gif");
    Model GetModel(string modelName) => contentManager.Load<Model>(modelName);
    SoundEffect GetSoundEffect(string soundEffectName) => contentManager.Load<SoundEffect>(soundEffectName);
    SpriteFont GetSpriteFont(string spriteFontName) => contentManager.Load<SpriteFont>(spriteFontName);

    public T Get<T>(string name)
    {
        if (typeof(T) == typeof(Texture2D)) return (T)(object)GetTexture(name);
        if (typeof(T) == typeof(Image<Rgba32>)) return (T)(object)GetImage(name);
        if (typeof(T) == typeof(Model)) return (T)(object)GetModel(name);
        if (typeof(T) == typeof(SoundEffect)) return (T)(object)GetSoundEffect(name);
        if (typeof(T) == typeof(SpriteFont)) return (T)(object)GetSpriteFont(name);

        throw new NotSupportedException($"{typeof(T).Name} can't be loaded by the Content Provider");
    }

    public Dictionary<string, T> GetAll<T>(params string[] names) => names.SelectAsDictionary(name => Get<T>(name));

    public IndexBuffer CreateIndexBuffer<T>(T[] indicees) where T : struct
    {
        var buffer = new IndexBuffer(graphicsDevice, typeof(T) == typeof(int) ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits, indicees.Length, BufferUsage.WriteOnly);
        buffer.SetData(indicees);
        return buffer;
    }

    public VertexBuffer CreateVertexBuffer<T>(T[] vertices) where T : struct
    {
        var buffer = new VertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.WriteOnly);
        buffer.SetData(vertices);
        return buffer;
    }
}
