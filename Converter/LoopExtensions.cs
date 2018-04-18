using System;
using System.Threading.Tasks;

namespace Converter
{
    static class LoopExtensions
    {
        public static void ForEach<T>(this T[,] array, Func<int, int, T, T> compute)
        {
            for (var y = 0; y < array.GetLength(1); y++)
            {
                for (var x = 0; x < array.GetLength(0); x++)
                {
                    array[x, y] = compute(x, y, array[x, y]);
                }
            }
        }

        public static void ForEach<T>(this T[,] array, Action<int, int, T> compute)
        {
            for (var y = 0; y < array.GetLength(1); y++)
            {
                for (var x = 0; x < array.GetLength(0); x++)
                {
                    compute(x, y, array[x, y]);
                }
            }
        }

        public static void ForEach<T>(this T[,] array, int minx, int maxx, int miny, int maxy, Action<int, int, T> compute)
        {
            for (var y = Math.Max(miny, 0); y < Math.Min(maxy, array.GetLength(1)); y++)
            {
                for (var x = Math.Max(minx, 0); x < Math.Min(maxx, array.GetLength(0)); x++)
                {
                    compute(x, y, array[x, y]);
                }
            }
        }

        public static void ForEach<T>(this T[,] array, int centerx, int centery, int range, Action<int, int, T> compute)
        {
            var sizex = array.GetLength(0);
            var sizey = array.GetLength(1);
            var minx = centerx - range;
            var maxx = centerx + range;
            var miny = centery - range;
            var maxy = centery + range;
            for (var y = miny; y < maxy; y++)
            {
                for (var x = minx; x < maxx; x++)
                {
                    var _x = ((x % sizex) + sizex) % sizex;
                    var _y = ((y % sizey) + sizey) % sizey;
                    compute(x, y, array[_x, _y]);
                }
            }
        }

        public static void ForEach<T>(this T[,,] array, Action<int, int, int, T> compute)
        {
            for (var z = 0; z < array.GetLength(2); z++)
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    for (var x = 0; x < array.GetLength(0); x++)
                    {
                        compute(x, y, z, array[x, y, z]);
                    }
                }
            }
        }

        public static void ForEach<T>(this T[,,] array, Func<int, int, int, T, T> compute, Action<float> onPercentageStep = null)
        {
            var elements = array.Length;
            var percentage = -1;
            var processed = 0;
            for (var z = 0; z < array.GetLength(2); z++)
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    for (var x = 0; x < array.GetLength(0); x++)
                    {
                        array[x, y, z] = compute(x, y, z, array[x, y, z]);
                        processed++;
                        var nextPercentage = 100 * processed / elements;
                        if (nextPercentage != percentage)
                        {
                            percentage = nextPercentage;
                            onPercentageStep?.Invoke(percentage);
                        }
                    }
                }
            }
        }

        public static void ForEachParallel<T>(this T[,,] array, Func<int, int, int, T, T> compute, Action<float> onPercentageStep = null)
        {
            var elements = array.Length;
            var percentage = -1;
            var processed = 0;
            Parallel.For(0, array.GetLength(2), z =>
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    for (var x = 0; x < array.GetLength(0); x++)
                    {
                        array[x, y, z] = compute(x, y, z, array[x, y, z]);
                        processed++;
                        var nextPercentage = 100 * processed / elements;
                        if (nextPercentage != percentage)
                        {
                            percentage = nextPercentage;
                            onPercentageStep?.Invoke(percentage);
                        }
                    }
                }
            });
        }
    }
}
