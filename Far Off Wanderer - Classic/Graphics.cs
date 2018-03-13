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
            if (Textures.ContainsKey(textureName) == false)
            {
                Textures[textureName] = ContentManager.Load<Texture2D>(textureName);
            }
            return Textures[textureName];
        }

        public Model GetModel(string modelName)
        {
            if (Models.ContainsKey(modelName) == false)
            {
                Models[modelName] = ContentManager.Load<Model>(modelName);
            }
            return Models[modelName];
        }

        Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        Dictionary<string, Model> Models = new Dictionary<string, Model>();
    }
}