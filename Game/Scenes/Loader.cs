using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Far_Off_Wanderer
{
    namespace Scenes
    {
        static class Loader
        {
            static readonly TypeInfo[] SceneTypes = typeof(Scene).GetTypeInfo().Assembly.DefinedTypes
                       .Where(t => t.IsSubclassOf(typeof(Scene))).ToArray();

            class BaseScene : Scene
            {
            }

            static Scene GetScene(string name)
            {
                var path = Path.Combine(System.Environment.CurrentDirectory, "Content", "Scenes", $"{name}.json");
                var content = File.ReadAllText(path);

                var scene = Convert.Json.Convert<BaseScene>(content);
                var foundType = SceneTypes.FirstOrDefault(t => string.Equals(t.Name, scene.Type, StringComparison.OrdinalIgnoreCase));
                if (foundType != null)
                {
                    var loaded = (Scene)Convert.Json.Convert(content, foundType.AsType());
                    loaded.Name = name;
                    return loaded;
                }
                else
                {
                    throw new Exception($"Scene Type '{scene.Type}' is not known");
                }
            }

            public static All GetAll()
            {
                var path = Path.Combine(System.Environment.CurrentDirectory, "Content", "Scenes");
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