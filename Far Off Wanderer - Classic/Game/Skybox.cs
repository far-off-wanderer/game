using Microsoft.Xna.Framework;

namespace Far_Off_Wanderer
{
    public class Skybox : Object3D
    {
        Color color;

        public Color Color => color;

        public Skybox(Color color)
        {
            this.color = color;
        }
    }
}