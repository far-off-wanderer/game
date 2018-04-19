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

        public static DistanceField FromHeightmap(float[,] heightmap, int height)
        {
            var size = heightmap.GetLength(0);
            var field = new DistanceField(size, height, size);

            var oneOverSize = 1f / size;

            field.Field.ForEachParallel((x, y, z, value) =>
            {
                var locationx = x * oneOverSize;
                var locationy = y * oneOverSize;
                var locationz = z * oneOverSize;

                var shortestDistance = float.PositiveInfinity;

                // in 'grid-cell space'
                var range = (int)Math.Ceiling(y - height * heightmap[x, z]);

                if (range > 0)
                {
                    var sizex = heightmap.GetLength(0);
                    var sizey = heightmap.GetLength(1);
                    var minu = x - range;
                    var maxu = x + range;
                    var minv = y - range;
                    var maxv = y + range;
                    for (var v = minv; v < maxv; v++)
                    {
                        for (var u = minu; u < maxu; u++)
                        {
                            var _x = ((x % sizex) + sizex) % sizex;
                            var _y = ((y % sizey) + sizey) % sizey;

                            var h = heightmap[_x, _y];

                            var pointx = u * oneOverSize;
                            var pointy = height * h * oneOverSize;
                            var pointz = v * oneOverSize;
                            var dx = pointx - locationx;
                            var dz = pointz - locationz;
                            if (pointy > locationy)
                            {
                                var _distance = dx * dx + dz * dz;
                                if(_distance < shortestDistance)
                                {
                                    shortestDistance = _distance;
                                }
                            }
                            else
                            {
                                var dy = pointy - locationy;
                                var _distance = dx * dx + dy * dy + dz * dz;
                                if(_distance < shortestDistance)
                                {
                                    shortestDistance = _distance;
                                }
                            }
                        }
                    }
                }

                if (shortestDistance == float.PositiveInfinity)
                {
                    shortestDistance = 0;
                }

                return (float)Math.Sqrt(shortestDistance);
            });

            return field;
        }
    }
}
