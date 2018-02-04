using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Conesoft.Engine
{
    public static class DimensionalArrayHelpers
    {
        public static int LengthX<T>(this T[,,] array) => array.GetLength(0);
        public static int LengthY<T>(this T[,,] array) => array.GetLength(1);
        public static int LengthZ<T>(this T[,,] array) => array.GetLength(2);

        public static Vector3 Lengths<T>(this T[,,] array) => new Vector3(array.LengthX(), array.LengthY(), array.LengthZ());

        public static void Set<T>(this T[,,] array, T value)
        {
            array.Set(_ => value);
        }

        public static void Set<T>(this T[,,] array, Func<T, T> computation)
        {
            array.Set((current, _x, _y, _z) => computation(current));
        }

        public static void Set<T>(this T[,,] array, Func<T, int, int, int, T> computation)
        {
            for (var z = 0; z < array.LengthZ(); z++)
            {
                for (var y = 0; y < array.LengthY(); y++)
                {
                    for (var x = 0; x < array.LengthX(); x++)
                    {
                        array[x, y, z] = computation(array[x, y, z], x, y, z);
                    }
                }
            }
        }

        public static void SetInParallel<T>(this T[,,] array, Func<T, int, int, int, T> computation)
        {
            Parallel.For(0, array.LengthZ(), z =>
            {
                for (var y = 0; y < array.LengthY(); y++)
                {
                    for (var x = 0; x < array.LengthX(); x++)
                    {
                        array[x, y, z] = computation(array[x, y, z], x, y, z);
                    }
                }
            });
        }

        public static void Get<T>(this T[,,] array, Action<T> computation)
        {
            array.Get((value, x, y, z) => computation(value));
        }

        public static void Get<T>(this T[,,] array, Action<T, int, int, int> computation)
        {
            for (var z = 0; z < array.LengthZ(); z++)
            {
                for (var y = 0; y < array.LengthY(); y++)
                {
                    for (var x = 0; x < array.LengthX(); x++)
                    {
                        computation(array[x, y, z], x, y, z);
                    }
                }
            }
        }
    }
}
