using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public class Graphics
    {
        public GraphicsDevice GraphicsDevice { get; set; }
        public ContentManager ContentManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }

        public Texture2D GetTexture(string textureName)
        {
            if (textures.ContainsKey(textureName) == false)
            {
                textures[textureName] = ContentManager.Load<Texture2D>(textureName);
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
            if(soundEffects.ContainsKey(soundEffectName) == false)
            {
                soundEffects[soundEffectName] = ContentManager.Load<SoundEffect>(soundEffectName);
            }
            return soundEffects[soundEffectName];
        }

        public SpriteFont GetSpriteFont(string spriteFontName)
        {
            if(spriteFonts.ContainsKey(spriteFontName) == false)
            {
                spriteFonts[spriteFontName] = ContentManager.Load<SpriteFont>(spriteFontName);
            }
            return spriteFonts[spriteFontName];
        }

        public Terrain GetTerrain(string terrainName)
        {
            if(terrains.ContainsKey(terrainName) == false)
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