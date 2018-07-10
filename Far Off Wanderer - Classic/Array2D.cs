using System;

namespace Far_Off_Wanderer
{
    public static class Array2D
    {
        public static T[,] Create<T>(int width, int height, Func<int, int, T> creator)
        {
            var array = new T[width, height];
            array.ForEach((x, y, _) => creator(x, y));
            return array;
        }
    }
}
