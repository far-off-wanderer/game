using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class GlobalContent
    {
        readonly ContentManager contentManager;
        readonly GraphicsDevice graphicsDevice;

        public GlobalContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            this.contentManager = contentManager;
            this.graphicsDevice = graphicsDevice;
        }

        public Content For(string sceneName) => new Content(this, sceneName);

        public T Load<T>(string name, string sceneName, Func<Stream, T> loader, params string[] extensions)
        {
            var path = Path.Combine(contentManager.RootDirectory, "Scenes", sceneName);

            var searchPath = Path.Combine(path, name + ".*");

            var pathToVerify = Path.GetDirectoryName(searchPath);

            if (Directory.Exists(pathToVerify))
            {
                var file = Directory.EnumerateFiles(path, $"{name}.*").Where(f => extensions.Contains(Path.GetExtension(f))).FirstOrDefault();
                if (file != null)
                {
                    return loader(File.OpenRead(file));
                }
            }
            return contentManager.Load<T>(name);
        }

        Texture2D GetTexture(string textureName, string sceneName) => Load(textureName, sceneName, stream => Texture2D.FromStream(graphicsDevice, stream), ".png", ".jpg");
        Image<Rgba32> GetImage(string imageName, string sceneName) => Load(imageName, sceneName, stream => Image.Load<Rgba32>(stream), ".png", ".jpg", ".bmp", ".gif");
        Model GetModel(string modelName) => contentManager.Load<Model>(modelName);
        SoundEffect GetSoundEffect(string soundEffectName) => contentManager.Load<SoundEffect>(soundEffectName);
        SpriteFont GetSpriteFont(string spriteFontName) => contentManager.Load<SpriteFont>(spriteFontName);

        public T Get<T>(string name, string sceneName)
        {
            if (typeof(T) == typeof(Texture2D)) return (T)(object)GetTexture(name, sceneName);
            if (typeof(T) == typeof(Image<Rgba32>)) return (T)(object)GetImage(name, sceneName);
            if (typeof(T) == typeof(Model)) return (T)(object)GetModel(name);
            if (typeof(T) == typeof(SoundEffect)) return (T)(object)GetSoundEffect(name);
            if (typeof(T) == typeof(SpriteFont)) return (T)(object)GetSpriteFont(name);

            throw new NotSupportedException($"{typeof(T).Name} can't be loaded by the Content Provider");
        }

        public IndexBuffer CreateIndexBuffer<T>(T[] indicees) where T : struct
        {
            var buffer = new IndexBuffer(graphicsDevice, typeof(T) == typeof(Int32) ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits, indicees.Length, BufferUsage.WriteOnly);
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
}