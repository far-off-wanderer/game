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
            }

            public class On_
            {
                public string Won { get; set; }
                public string GameOver { get; set; }
            }

            public Environment_ Environment { get; set; }
            public Surface_ Surface { get; set; }
            public On_ On { get; set; }
        }
    }
}