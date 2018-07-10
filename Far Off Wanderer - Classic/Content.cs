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
    public class Content
    {
        public ContentManager ContentManager { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public string Scene { get; set; }

        private T Load<T>(string name, Func<Stream, T> loader, params string[] extensions)
        {
            var path = Path.Combine(ContentManager.RootDirectory, "Scenes", Scene);

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
            return ContentManager.Load<T>(name);
        }

        public Texture2D GetTexture(string textureName) => Load(textureName, stream => Texture2D.FromStream(GraphicsDevice, stream), ".png", ".jpg");
        public Image<Rgba32> GetImage(string imageName) => Load(imageName, stream => Image.Load<Rgba32>(stream), ".png", ".jpg", ".bmp", ".gif");
        public Model GetModel(string modelName) => ContentManager.Load<Model>(modelName);
        public SoundEffect GetSoundEffect(string soundEffectName) => ContentManager.Load<SoundEffect>(soundEffectName);
        public SpriteFont GetSpriteFont(string spriteFontName) =>ContentManager.Load<SpriteFont>(spriteFontName);

        public T Get<T>(string name)
        {
            if (typeof(T) == typeof(Texture2D)) return (T)(object)GetTexture(name);
            if (typeof(T) == typeof(Image<Rgba32>)) return (T)(object)GetImage(name);
            if (typeof(T) == typeof(Model)) return (T)(object)GetModel(name);
            if (typeof(T) == typeof(SoundEffect)) return (T)(object)GetSoundEffect(name);
            if (typeof(T) == typeof(SpriteFont)) return (T)(object)GetSpriteFont(name);

            throw new NotSupportedException($"{typeof(T).Name} can't be loaded by the Content Provider");
        }
    }
}