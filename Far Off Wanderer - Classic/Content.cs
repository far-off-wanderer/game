using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

            if (Directory.Exists(path))
            {
                try
                {
                    var file = Directory.EnumerateFiles(path, $"{name}.*").Where(f => extensions.Contains(Path.GetExtension(f))).FirstOrDefault();
                    if (file != null)
                    {
                        return loader(File.OpenRead(file));
                    }
                } catch(Exception)
                {
                }
            }
            return ContentManager.Load<T>(name);
        }

        public Texture2D GetTexture(string textureName)
        {
            if (textures.ContainsKey(textureName) == false)
            {
                textures[textureName] = Load(textureName, stream => Texture2D.FromStream(GraphicsDevice, stream), ".png", ".jpg");
            }
            return textures[textureName];
        }

        public Model GetModel(string modelName)
        {
            if (models.ContainsKey(modelName) == false)
            {
                models[modelName] = ContentManager.Load<Model>(modelName);
            }
            return models[modelName];
        }

        public SoundEffect GetSoundEffect(string soundEffectName)
        {
            if (soundEffects.ContainsKey(soundEffectName) == false)
            {
                soundEffects[soundEffectName] = ContentManager.Load<SoundEffect>(soundEffectName);
            }
            return soundEffects[soundEffectName];
        }

        public SpriteFont GetSpriteFont(string spriteFontName)
        {
            if (spriteFonts.ContainsKey(spriteFontName) == false)
            {
                spriteFonts[spriteFontName] = ContentManager.Load<SpriteFont>(spriteFontName);
            }
            return spriteFonts[spriteFontName];
        }

        public Terrain GetTerrain(string terrainName)
        {
            if (terrains.ContainsKey(terrainName) == false)
            {
                //terrains[terrainName] = something;
            }
            return terrains[terrainName];
        }

        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        Dictionary<string, Model> models = new Dictionary<string, Model>();
        Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
        Dictionary<string, SpriteFont> spriteFonts = new Dictionary<string, SpriteFont>();
        Dictionary<string, Terrain> terrains = new Dictionary<string, Terrain>();
    }
}