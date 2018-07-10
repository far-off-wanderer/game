using System.IO;
using System.IO.Compression;

namespace Far_Off_Wanderer
{
    static class DistanceFieldFileExtensions
    {
        static public void SaveTo(this DistanceField distanceField, Stream output)
        {
            var shortenedVolume = new ushort[distanceField.Width, distanceField.Height, distanceField.Length];
            shortenedVolume.ForEach((x, y, z) => (ushort)(distanceField.Distance[x, y, z] * ushort.MaxValue));

            using (var compressed = new GZipStream(output, CompressionLevel.Optimal))
            using (var writer = new BinaryWriter(compressed))
            {
                writer.Write(distanceField.Width);
                writer.Write(distanceField.Height);
                writer.Write(distanceField.Length);
                shortenedVolume.ForEach((x, y, z, value) => writer.Write(value));
            }
        }
    }
}
