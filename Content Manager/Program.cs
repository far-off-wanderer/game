using Conesoft.Files;
using Humanizer;
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
            var directory = Directory.Common.Current;
            while (directory.Filtered("*.sln", allDirectories: false).Any() == false)
            {
                directory = directory.Parent;
            }
            var source = directory / "Content" / "src";
            var destiny = directory / "Content" / "bin";

            var vessels = source / "vessels";

            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            foreach (var item in await vessels.Filtered("*.json", allDirectories: false).ReadFromJson<ContentTypes.Vessel>(options))
            {
                Console.WriteLine($"file: {item.Name}");

                var zipped = destiny / vessels.Name / Filename.From(item.Content.Name, "vessel");
                {
                    var zip = zipped.AsNewZip();
                    zip["mesh"] = (await (item.Parent / Filename.FromExtended(item.Content.Mesh)).ReadFromWavefrontObjFormat()).GetBytes();
                    zip["albedo"] = await (item.Parent / Filename.FromExtended(item.Content.Textures.Albedo)).ReadBytes();
                }

                Console.WriteLine($"\tsize: {zipped.Info.Length.Bytes().Humanize("#.#0")}");
            }
        }
    }

    record Vertex(Vector3 Position, Vector2 TextureCoordinate, Vector3 Normal);

    static class FiletypeObjReader
    {
        public static async Task<Vertex[]> ReadFromWavefrontObjFormat(this File file)
        {
            var lines = await file.ReadLines();

            static float[] ParsedNumbersFromLine(string line) => line
                .Split(' ')
                .Skip(1)
                .Select(float.Parse)
                .ToArray();

            static int[] ParsedIntegersFromFaceEdge(string edge) => edge
                .Split('/')
                .Select(int.Parse)
                .Select(i => i - 1)
                .ToArray();

            var positions = lines
                .Where(l => l.StartsWith("v "))
                .Select(ParsedNumbersFromLine)
                .Select(s => new Vector3(s[0], s[1], s[2]))
                .ToArray();

            var normals = lines
                .Where(l => l.StartsWith("vn "))
                .Select(ParsedNumbersFromLine)
                .Select(s => new Vector3(s[0], s[1], s[2]))
                .ToArray();

            var textureCoordinates = lines
                .Where(l => l.StartsWith("vt "))
                .Select(ParsedNumbersFromLine)
                .Select(s => new Vector2(s[0], s[1]))
                .ToArray();

            return lines
                .Where(l => l.StartsWith("f "))
                .SelectMany(l => l.Split(' ').Skip(1).ToArray())
                .Select(ParsedIntegersFromFaceEdge)
                .Select(v => new Vertex(positions[v[0]], textureCoordinates[v[1]], normals[v[2]]))
                .ToArray();
        }
    }

    static class FiletypeMeshWriter
    {
        static byte[] GetBytesV3(Vector3 v) => BitConverter.GetBytes(v.X).Concat(BitConverter.GetBytes(v.Y)).Concat(BitConverter.GetBytes(v.Z)).ToArray();
        static byte[] GetBytesV2(Vector2 v) => BitConverter.GetBytes(v.X).Concat(BitConverter.GetBytes(v.Y)).ToArray();
        static byte[] GetBytes(Vertex v) => GetBytesV3(v.Position).Concat(GetBytesV3(v.Normal)).Concat(GetBytesV2(v.TextureCoordinate)).ToArray();
        public static byte[] GetBytes(this Vertex[] vertices) => vertices.SelectMany(GetBytes).ToArray();
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