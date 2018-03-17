using System;

namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public class Menu : Scene
        {
            public BackgroundLayouts Background { get; set; }
            public string NextScene { get; set; }
            public int Timeout { get; set; }

            public class BackgroundLayouts
            {
                public BackgroundLayoutImage Portrait { get; set; }
                public BackgroundLayoutImage Landscape { get; set; }
            }

            public class BackgroundLayoutImage
            {
                public string Name { get; set; }
                public int? Width { get; set; }
                public int? Height { get; set; }
            }
        }
    }
}