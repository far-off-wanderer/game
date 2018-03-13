namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public class Level : Scene
        {
            public class Environment_
            {
                public string BackgroundColor { get; set; }
                public bool Fog { get; set; }
            }

            public Environment_ Environment { get; set; }

        }
    }
}