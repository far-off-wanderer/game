using System;
using System.Numerics;

namespace Converter
{
    class DistanceField
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length { get; private set; }

        public float[,,] Field { get; private set; }

        public DistanceField(int width, int height, int length)
        {
            this.Width = width;
            this.Height = height;
            this.Length = length;

            this.Field = new float[width, height, length];
        }

        public static DistanceField FromHeightmap(float[,] heightmap, int height, Action<float> onPercentageStep = null)
        {
            var size = heightmap.GetLength(0);
            var field = new DistanceField(size, height, size);

            field.Field.ForEachParallel((x, y, z, value) =>
            {
                var location = new Vector3(x, y, z) / size;

                var shortestDistance = float.PositiveInfinity;

                // in 'grid-cell space'
                var range = (int)Math.Ceiling(y - height * heightmap[x, z]);

                if (range > 0)
                {
                    heightmap.ForEach(x - range, x + range, z - range, z + range, (u, v, h) =>
                    {
                        var point = new Vector3(u / (float)size, height * h / (float)size, v / (float)size);
                        if (point.Y > location.Y)
                        {
                            shortestDistance = Math.Min(shortestDistance, Vector2.DistanceSquared(new Vector2(location.X, location.Z), new Vector2(point.X, point.Z)));
                        }
                        else
                        {
                            shortestDistance = Math.Min(shortestDistance, Vector3.DistanceSquared(location, point));
                        }
                    });
                }

                if (shortestDistance == float.PositiveInfinity)
                {
                    shortestDistance = 0;
                }

                return (float)Math.Sqrt(shortestDistance);
            }, onPercentageStep);

            return field;
        }
    }
}
