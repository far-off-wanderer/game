using Microsoft.Xna.Framework;
using System.Globalization;

namespace Far_Off_Wanderer
{
    static class ColorHelper
    {
        public static Color ToColor(this string hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), NumberStyles.HexNumber);

            return new Color(r, g, b, a);
        }
    }
}