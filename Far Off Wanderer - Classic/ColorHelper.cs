using Microsoft.Xna.Framework;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation.HsvColorSapce;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Far_Off_Wanderer
{
    static class ColorHelper
    {
        public static Color ToColor(this string color)
        {
            if (color.StartsWith("#"))
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

        public static Color Complementary(this Color color)
        {
            var converter = new HsvAndRgbConverter();
            var hsv = converter.Convert(color);
            return converter.Convert(new Hsv(hsv.H + 180, hsv.S, hsv.V));
        }

        static Vector3 greyscale = new Vector3(.3f, .59f, .11f);

        public static Color GreyedOut(this Color color, float amount)
        {
            var c = color.ToVector3();
            var grey = Vector3.Dot(c, greyscale);

            return new Color(
                r: c.X * (1 - amount) + grey * amount,
                g: c.Y * (1 - amount) + grey * amount,
                b: c.Z * (1 - amount) + grey * amount
            );
        }

        public static Color Faded(this Color color, float fade)
        {
            return new Color(color, color.ToVector4().W * fade);
        }
    }
}

// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.


namespace SixLabors.ImageSharp.ColorSpaces
{
    /// <summary>
    /// Represents a HSV (hue, saturation, value) color. Also known as HSB (hue, saturation, brightness).
    /// </summary>
    internal struct Hsv : IEquatable<Hsv>
    {
        /// <summary>
        /// Max range used for clamping.
        /// </summary>
        private static readonly Vector3 VectorMax = new Vector3(360, 1, 1);

        /// <summary>
        /// The backing vector for SIMD support.
        /// </summary>
        private readonly Vector3 backingVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hsv"/> struct.
        /// </summary>
        /// <param name="h">The h hue component.</param>
        /// <param name="s">The s saturation component.</param>
        /// <param name="v">The v value (brightness) component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Hsv(float h, float s, float v)
            : this(new Vector3(h, s, v))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hsv"/> struct.
        /// </summary>
        /// <param name="vector">The vector representing the h, s, v components.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Hsv(Vector3 vector)
        {
            this.backingVector = Vector3.Clamp(vector, Vector3.Zero, VectorMax);
        }

        /// <summary>
        /// Gets the hue component.
        /// <remarks>A value ranging between 0 and 360.</remarks>
        /// </summary>
        public float H
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.backingVector.X;
        }

        /// <summary>
        /// Gets the saturation component.
        /// <remarks>A value ranging between 0 and 1.</remarks>
        /// </summary>
        public float S
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.backingVector.Y;
        }

        /// <summary>
        /// Gets the value (brightness) component.
        /// <remarks>A value ranging between 0 and 1.</remarks>
        /// </summary>
        public float V
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.backingVector.Z;
        }

        /// <inheritdoc/>
        public Vector3 Vector
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.backingVector;
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="Rgba32"/> to a
        /// <see cref="Hsv"/>.
        /// </summary>
        /// <param name="color">The instance of <see cref="Rgba32"/> to convert.</param>
        /// <returns>
        /// An instance of <see cref="Hsv"/>.
        /// </returns>
        public static implicit operator Hsv(Rgba32 color)
        {
            float r = color.R / 255F;
            float g = color.G / 255F;
            float b = color.B / 255F;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float chroma = max - min;
            float h = 0;
            float s = 0;
            float v = max;

            if (Math.Abs(chroma) < float.Epsilon)
            {
                return new Hsv(0, s, v);
            }

            if (Math.Abs(r - max) < float.Epsilon)
            {
                h = (g - b) / chroma;
            }
            else if (Math.Abs(g - max) < float.Epsilon)
            {
                h = 2 + ((b - r) / chroma);
            }
            else if (Math.Abs(b - max) < float.Epsilon)
            {
                h = 4 + ((r - g) / chroma);
            }

            h *= 60;
            if (h < 0.0)
            {
                h += 360;
            }

            s = chroma / v;

            return new Hsv(h, s, v);
        }

        /// <summary>
        /// Compares two <see cref="Hsv"/> objects for equality.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Hsv"/> on the left side of the operand.
        /// </param>
        /// <param name="right">
        /// The <see cref="Hsv"/> on the right side of the operand.
        /// </param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Hsv left, Hsv right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="Hsv"/> objects for inequality.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Hsv"/> on the left side of the operand.
        /// </param>
        /// <param name="right">
        /// The <see cref="Hsv"/> on the right side of the operand.
        /// </param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Hsv left, Hsv right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return this.backingVector.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Hsv other && this.Equals(other);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Hsv other)
        {
            return this.backingVector.Equals(other.backingVector);
        }
    }
}

// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.


namespace SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation.HsvColorSapce
{
    /// <summary>
    /// Color converter between HSV and Rgb
    /// See <see href="http://www.poynton.com/PDFs/coloureq.pdf"/> for formulas.
    /// </summary>
    internal class HsvAndRgbConverter
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Convert(Hsv input)
        {
            float s = input.S;
            float v = input.V;

            if (Math.Abs(s) < float.Epsilon)
            {
                return new Color(v, v, v);
            }

            float h = (Math.Abs(input.H - 360) < float.Epsilon) ? 0 : input.H / 60;
            int i = (int)Math.Truncate(h);
            float f = h - i;

            float p = v * (1F - s);
            float q = v * (1F - (s * f));
            float t = v * (1F - (s * (1F - f)));

            float r, g, b;
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;

                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(r, g, b);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Hsv Convert(Color input)
        {
            float r = input.ToVector3().X;
            float g = input.ToVector3().Y;
            float b = input.ToVector3().Z;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float chroma = max - min;
            float h = 0;
            float s = 0;
            float v = max;

            if (Math.Abs(chroma) < float.Epsilon)
            {
                return new Hsv(0, s, v);
            }

            if (Math.Abs(r - max) < float.Epsilon)
            {
                h = (g - b) / chroma;
            }
            else if (Math.Abs(g - max) < float.Epsilon)
            {
                h = 2 + ((b - r) / chroma);
            }
            else if (Math.Abs(b - max) < float.Epsilon)
            {
                h = 4 + ((r - g) / chroma);
            }

            h *= 60;
            if (h < 0.0)
            {
                h += 360;
            }

            s = chroma / v;

            return new Hsv(h, s, v);
        }
    }
}