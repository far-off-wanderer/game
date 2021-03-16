using Conesoft.Files;
using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Content_Manager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = Directory.From(@"C:\Users\spoda\source\repos\far-off-wanderer\game") / "Content" / "src";
            var destiny = Directory.From(@"C:\Users\spoda\source\repos\far-off-wanderer\game") / "Content" / "bin";

            var vessels = source / "vessels";

            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            foreach (var item in await vessels.Filtered("*.json", allDirectories: false).ReadFromJson<ContentTypes.Vessel>(options))
            {
                Console.WriteLine($"file: {item.Name}");
                Console.WriteLine($"name: {item.Content.Name}");

                var lines = await (item.Parent / Filename.FromExtended(item.Content.Mesh)).ReadLines();

                var positions = lines.Where(l => l.StartsWith("v ")).Select(line =>
                {
                    var s = line.Split(' ').Skip(1).Select(s => float.Parse(s)).ToArray();
                    return new Vector3(s[0], s[1], s[2]);
                }).ToArray();

                var normals = lines.Where(l => l.StartsWith("vn ")).Select(line =>
                {
                    var s = line.Split(' ').Skip(1).Select(s => float.Parse(s)).ToArray();
                    return new Vector3(s[0], s[1], s[2]);
                }).ToArray();

                var textureCoordinates = lines.Where(l => l.StartsWith("vt ")).Select(line =>
                {
                    var s = line.Split(' ').Skip(1).Select(s => float.Parse(s)).ToArray();
                    return new Vector2(s[0], s[1]);
                }).ToArray();

                var faces = lines.Where(l => l.StartsWith("f ")).Select(line =>
                {
                    var s = line.Split(' ').Skip(1).ToArray();

                    var a = s[0].Split('/').Select(s => int.Parse(s) - 1).ToArray();
                    var b = s[1].Split('/').Select(s => int.Parse(s) - 1).ToArray();
                    var c = s[2].Split('/').Select(s => int.Parse(s) - 1).ToArray();

                    return new Face
                    {
                        A = new Vertex
                        {
                            Position = positions[a[0]],
                            TextureCoordinate = textureCoordinates[a[1]],
                            Normal = normals[a[2]]
                        },
                        B = new Vertex
                        {
                            Position = positions[b[0]],
                            TextureCoordinate = textureCoordinates[b[1]],
                            Normal = normals[b[2]]
                        },
                        C = new Vertex
                        {
                            Position = positions[c[0]],
                            TextureCoordinate = textureCoordinates[c[1]],
                            Normal = normals[c[2]]
                        },
                    };
                }).ToArray();

                Console.WriteLine($"\tvertices:  {positions.Length}");
                Console.WriteLine($"\tnormals:   {normals.Length}");
                Console.WriteLine($"\ttexcoords: {textureCoordinates.Length}");
                Console.WriteLine($"\tfaces:     {faces.Length}");

                Console.WriteLine();
            }
        }
    }

    record Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
    }

    record Face
    {
        public Vertex A;
        public Vertex B;
        public Vertex C;
    }

    namespace ContentTypes
    {
        public record Vessel
        {
            public string Name;
            public string Mesh;
            public Textures_ Textures;

            public record Textures_
            {
                public string Albedo;
            }
        }
    }
}