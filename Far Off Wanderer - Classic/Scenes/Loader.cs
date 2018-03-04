using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;

namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        public static class Loader
        {
            public static TypeInfo[] SceneTypes = typeof(Scene).GetTypeInfo().Assembly.DefinedTypes
                       .Where(t => t.IsSubclassOf(typeof(Scene))).ToArray();

            public static Scene GetScene(string name)
            {
                var path = Path.Combine(Package.Current.InstalledLocation.Path, "Content", "Scenes", $"{name}.json");
                var content = File.ReadAllText(path);

                var scene = Newtonsoft.Json.JsonConvert.DeserializeObject<Scene>(content);
                var foundTypes = SceneTypes.Where(t => string.Equals(t.Name, scene.Type, StringComparison.OrdinalIgnoreCase));
                if (foundTypes.Any())
                {
                    return (Scene)Newtonsoft.Json.JsonConvert.DeserializeObject(content, foundTypes.First().AsType());
                }
                else
                {
                    throw new Exception($"Scene Type '{scene.Type}' is not known");
                }
            }

            public static All GetAll()
            {
                var path = Path.Combine(Package.Current.InstalledLocation.Path, "Content", "Scenes");
                var files = Directory.GetFiles(path).Where(p => Path.GetExtension(p) == ".json").Select(p => Path.GetFileNameWithoutExtension(p));

                var scenes = files.Select(f => new
                {
                    Key = f,
                    Value = GetScene(f)
                }).ToDictionary(item => item.Key, item => item.Value);

                return new All(scenes);
            }
        }
    }
}