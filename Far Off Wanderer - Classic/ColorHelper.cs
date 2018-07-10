using Microsoft.Xna.Framework;
using System.Globalization;
using System.Linq;

namespace Far_Off_Wanderer
{
    static class ColorHelper
    {
        public static Color ToColor(this string color)
        {
            if (color.StartsWith('#'))
            {
                //remove the # at the front
                color = color.Replace("#", "");

                byte a = 255;
                byte r = 255;
                byte g = 255;
                byte b = 255;

                int start = 0;

                //handle ARGB strings (8 characters long)
                if (color.Length == 8)
                {
                    a = byte.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                    start = 2;
                }

                //convert RGB characters to bytes
                r = byte.Parse(color.Substring(start, 2), NumberStyles.HexNumber);
                g = byte.Parse(color.Substring(start + 2, 2), NumberStyles.HexNumber);
                b = byte.Parse(color.Substring(start + 4, 2), NumberStyles.HexNumber);

                return new Color(r, g, b, a);
            }
            else
            {
                return (Color)(typeof(Color).GetProperties().FirstOrDefault(p => string.Equals(p.Name, color, System.StringComparison.OrdinalIgnoreCase)).GetValue(null));
            }
        }
    }
}