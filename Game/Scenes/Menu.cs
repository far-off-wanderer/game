namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public class Menu : Scene
        {
            public class On_
            {
                public string Next { get; set; }
                public string Cancel { get; set; }
            }

            public class Background_
            {
                public Image_ Portrait { get; set; }
                public Image_ Landscape { get; set; }
            }

            public class Image_
            {
                public string Name { get; set; }
                public int? Width { get; set; }
                public int? Height { get; set; }
            }

            public Background_ Background { get; set; }
            public On_ On { get; set; }
            public int Timeout { get; set; }
        }
    }
}