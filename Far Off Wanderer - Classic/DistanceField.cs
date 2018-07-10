using System;
using Microsoft.Xna.Framework;

namespace Far_Off_Wanderer
{
    public class DistanceField
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length { get; private set; }
        public Vector3 Size { get; private set; }

        public float[,,] Distance { get; private set; }
        public Vector3[,,] Nearest { get; private set; }

        public enum BorderBehaviour
        {
            Solid,
            Empty,
            Repeat
        }

        public BorderBehaviour LeftBorder { get; private set; }
        public BorderBehaviour RightBorder { get; private set; }
        public BorderBehaviour FrontBorder { get; private set; }
        public BorderBehaviour BackBorder { get; private set; }
        public BorderBehaviour TopBorder { get; private set; }
        public BorderBehaviour BottomBorder { get; private set; }

        public DistanceField(int width, int height, int length)
        {
            this.Width = width;
            this.Height = height;
            this.Length = length;
            this.Size = new Vector3(width, height, length);

            this.Distance = new float[width, height, length];
            this.Nearest = new Vector3[width, height, length];

            this.LeftBorder = BorderBehaviour.Repeat;
            this.RightBorder = BorderBehaviour.Repeat;
            this.FrontBorder = BorderBehaviour.Repeat;
            this.BackBorder = BorderBehaviour.Repeat;
            this.TopBorder = BorderBehaviour.Empty;
            this.BottomBorder = BorderBehaviour.Solid;
        }

        public static DistanceField FromHeightmap(float[,] heightmap, int height)
        {
            var size = heightmap.GetLength(0);
            var field = new DistanceField(size, height, size);

            var oneOverSize = 1f / size;

            field.Distance.ForEachParallel((x, y, z) =>
            {
                var locationx = x * oneOverSize;
                var locationy = y * oneOverSize;
                var locationz = z * oneOverSize;

                // in 'grid-cell space'
                var range = (int)MathF.Ceiling(y - height * heightmap[x, z]);

                if (range > 0)
                {
                    var minu = x - range;
                    var maxu = x + range;
                    var minv = y - range;
                    var maxv = y + range;
                    float shortestDistanceSquared = range * range;
                    var distancex = 0f;
                    var distancey = 0f;
                    var distancez = 0f;
                    for (var v = minv; v < maxv; v++)
                    {
                        var _v = (v + size) % size;
                        for (var u = minu; u < maxu; u++)
                        {
                            var _u = (u + size) % size;

                            var h = heightmap[_u, _v];

                            var pointx = u * oneOverSize;
                            var pointy = height * h * oneOverSize;
                            var pointz = v * oneOverSize;
                            var dx = pointx - locationx;
                            var dz = pointz - locationz;
                            if (pointy > locationy)
                            {
                                var _distanceSquared = dx * dx + dz * dz;
                                if(_distanceSquared < shortestDistanceSquared)
                                {
                                    shortestDistanceSquared = _distanceSquared;
                                    distancex = dx;
                                    distancey = 0;
                                    distancez = dz;
                                }
                            }
                            else
                            {
                                var dy = pointy - locationy;
                                var _distanceSquared = dx * dx + dy * dy + dz * dz;
                                if(_distanceSquared < shortestDistanceSquared)
                                {
                                    shortestDistanceSquared = _distanceSquared;
                                    distancex = dx;
                                    distancey = dy;
                                    distancez = dz;
                                }
                            }
                        }
                    }

                    field.Nearest[x, y, z].X = distancex;
                    field.Nearest[x, y, z].Y = distancey;
                    field.Nearest[x, y, z].Z = distancez;

                    return MathF.Sqrt(shortestDistanceSquared);
                }
                return 0;
            });

            return field;
        }

        int IMod(float a, int size) => (int)MathF.Floor(a - size * MathF.Floor(a / size));
        int IClamp(float a, int size) => a < 0 ? 0 : (a > size ? size : (int) a);

        public float DistanceAt(Vector3 at)
        {
            var at_ = (at + Vector3.One) * .5f;

            var x = IMod(at_.X * Width, Width);
            var y = IClamp(at_.Y * Height, Height);
            var z = IMod(at_.Z * Length, Length);

            var distance_ = Distance[x, y, z];

            var distance = distance_ * 2;

            return distance;
        }
    }
}
