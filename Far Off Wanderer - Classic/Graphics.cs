using Microsoft.Xna.Framework.Graphics;

namespace Far_Off_Wanderer
{
    public class Graphics
    {
        readonly GraphicsDevice graphicsDevice;
        readonly SpriteBatch spriteBatch;

        public GraphicsDevice GraphicsDevice => graphicsDevice;
        public SpriteBatch SpriteBatch => spriteBatch;

        public Graphics(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
        }
    }
}