using System;

namespace Converter
{
    public static class Array3D
    {
        public static T[,,] Create<T>(int width, int height, int depth, Func<int, int, int, T> creator)
        {
            var array = new T[width, height, depth];
            array.ForEach((x, y, z, _) => creator(x, y, z));
            return array;
        }
    }
}
