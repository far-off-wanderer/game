using System.IO;
using System.IO.Compression;

namespace Converter
{
    static class DistanceFieldFileExtensions
    {
        static public void SaveTo(this DistanceField distanceField, Stream output)
        {
            var shortenedVolume = new ushort[distanceField.Width, distanceField.Height, distanceField.Length];
            shortenedVolume.ForEach((x, y, z, _) => (ushort)(distanceField.Field[x, y, z] * ushort.MaxValue));

            using (var compressed = new GZipStream(output, CompressionLevel.Optimal))
            using (var writer = new BinaryWriter(compressed))
            {
                writer.Write(shortenedVolume.GetLength(0));
                writer.Write(shortenedVolume.GetLength(1));
                writer.Write(shortenedVolume.GetLength(2));
                shortenedVolume.ForEach((x, y, z, value) => writer.Write(value));
            }
        }
    }
}
