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

            var oneOverSize = 1f / size;

            field.Field.ForEach((x, y, z, value) =>
            {
                var location = (x: x * oneOverSize, y: y * oneOverSize, z: z * oneOverSize);

                var shortestDistance = float.PositiveInfinity;

                // in 'grid-cell space'
                var range = (int)Math.Ceiling(y - height * heightmap[x, z]);

                if (range > 0)
                {
                    heightmap.ForEach(x, z, range, (u, v, h) =>
                    {
                        var point = (x: u * oneOverSize, y: height * h * oneOverSize, z: v * oneOverSize);
                        var (dx, dy, dz) = (point.x - location.x, point.y - location.y, point.z - location.z);
                        if (point.y > location.y)
                        {
                            shortestDistance = Math.Min(shortestDistance, dx * dx + dz * dz);
                        }
                        else
                        {
                            shortestDistance = Math.Min(shortestDistance, dx * dx + dy * dy + dz * dz);
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
