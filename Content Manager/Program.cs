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

                using var zip = (destiny / vessels.Name / Filename.From(item.Content.Name, "vessel")).AsNewZip();
                zip["mesh"] = (await (item.Parent / Filename.FromExtended(item.Content.Mesh)).ReadFromWavefrontObjFormat()).GetBytes();
                zip["albedo"] = await (item.Parent / Filename.FromExtended(item.Content.Textures.Albedo)).ReadBytes();
            }
        }
    }

    class Zip
    {
    }

    record Vertex(Vector3 Position, Vector2 TextureCoordinate, Vector3 Normal);

    static class FiletypeObjReader
    {
        public static async Task<Vertex[]> ReadFromWavefrontObjFormat(this File file)
        {
            var lines = await file.ReadLines();

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

            return lines.Where(l => l.StartsWith("f ")).SelectMany(line =>
            {
                var s = line.Split(' ').Skip(1).ToArray();
                var all = s.Select(s => s.Split('/').Select(s => int.Parse(s) - 1).ToArray()).ToArray();
                return all.Select(v => new Vertex(positions[v[0]], textureCoordinates[v[1]], normals[v[2]])).ToArray();
            }).ToArray();
        }
    }

    static class FiletypeMeshWriter
    {
        public static byte[] GetBytes(this Vertex[] vertices)
        {
            byte[] GetBytesV3(Vector3 v) => BitConverter.GetBytes(v.X).Concat(BitConverter.GetBytes(v.Y)).Concat(BitConverter.GetBytes(v.Z)).ToArray();
            byte[] GetBytesV2(Vector2 v) => BitConverter.GetBytes(v.X).Concat(BitConverter.GetBytes(v.Y)).ToArray();
            byte[] GetBytes(Vertex v) => GetBytesV3(v.Position).Concat(GetBytesV3(v.Normal)).Concat(GetBytesV2(v.TextureCoordinate)).ToArray();

            return vertices.SelectMany(GetBytes).ToArray();
        }

        public static async Task WriteAsMesh(this File file, Vertex[] vertices)
        {
            await file.WriteBytes(vertices.GetBytes());
        }
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