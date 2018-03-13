using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public class All
        {
            ReadOnlyDictionary<string, Scene> scenes;

            public All(IDictionary<string, Scene> scenes)
            {
                this.scenes = new ReadOnlyDictionary<string, Scene>(scenes);
            }

            public IEnumerable<Scene> Scenes => scenes.Values;

            public Scene Index => scenes["index"];

            public static All Load() => Loader.GetAll();
        }
    }
}