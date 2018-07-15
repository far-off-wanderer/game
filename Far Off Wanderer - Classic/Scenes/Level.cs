using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public class Level : Scene
        {
            public class Environment_
            {
                public string BackgroundColor { get; set; }
                public float Fog { get; set; }
            }

            public class Surface_
            {
                public string Texture { get; set; }
                public string Color { get; set; }
                public int Size { get; set; }
                public int Height { get; set; }
                public Noise_ Noise { get; set; }
                public float? BorderToInfinity { get; set; }

                public class Noise_
                {
                    public float Top { get; set; }
                    public float Bottom { get; set; }
                }
            }

            public class On_
            {
                public string Won { get; set; }
                public string GameOver { get; set; }
                public string Cancel { get; set; }
            }

            public Environment_ Environment { get; set; }
            public Dictionary<string, Surface_> Surfaces { get; set; }
            public On_ On { get; set; }
        }
    }
}