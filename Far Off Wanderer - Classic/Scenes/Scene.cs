namespace Far_Off_Wanderer
{
    using System.Collections.Generic;

    namespace Scenes
    {
        public class Scene
        {
            public string Type { get; set; }
            public Dictionary<string, string> Next { get; set; }
        }
    }
}