using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Landscape : Object3D
    {
        private readonly string name;
        private readonly Color color;
        private readonly Image<Rgba32> map;
        private readonly int width;
        private readonly int height;
        private readonly int length;
        private readonly Vector3[,] pointmap;
        private readonly VertexBuffer vertexBuffer;
        private readonly IndexBuffer indexBuffer;

        public Color Color => color;
        public Vector3[,] Points => pointmap;
        public VertexBuffer VertexBuffer => vertexBuffer;
        public IndexBuffer IndexBuffer => indexBuffer;
        public int Resolution => width;

        public Landscape(string name, Content content, Image<Rgba32> map, float bottomNoise, float topNoise, Color color, float? borderToInfinity)
        {
            this.name = name;
            this.color = color;
            this.map = map;
            this.width = map.Width;
            this.length = map.Height;
            this.height = Math.Min(width, length) / 8;
            this.pointmap = new Vector3[width, length];
            var vertices = new VertexPositionNormalTexture[width * length];

            for (var z = 0; z < length; z++)
            {
                for (var x = 0; x < width; x++)
                {
                    var h = map[x, z].ToScaledVector4().X * height;
                    var p = new Vector3(1f * x / width, 1f * h / Math.Min(width, length), 1f * z / length);

                    if (borderToInfinity.HasValue)
                    {
                        var borderFraction = borderToInfinity.Value;
                        var fromcenter = 2 * p - Vector3.One;
                        fromcenter.Y = 0;
                        if (fromcenter.LengthSquared() > borderFraction)
                        {
                            p += (float)(Math.Tan((Math.PI / 2) * Math.Min(1, (fromcenter.LengthSquared() - borderFraction) / (1 - borderFraction)))) * Vector3.Normalize(fromcenter);
                        }
                    }

                    p += (1f / width) * Noise.Vector3.Get(x / 4, z / 4) * ((1 - h / height) * bottomNoise + (h / height) * topNoise);

                    pointmap[x, z] = p;

                    vertices[x + width * z] = new VertexPositionNormalTexture(pointmap[x, z], Vector3.Up, (4f / width) * new Vector2(1f * x / width, 1f * z / length));
                }
            }

            for (var z = 0; z < length; z++)
            {
                for (var x = 0; x < width; x++)
                {
                    var points = GetHeightsAround1D(1f * x / width, 1f * z / length, 3f / width);
                    var normals = points.Select(p =>
                    {
                        var toPoint = pointmap[x, z];
                        var toSide = Vector3.Normalize(Vector3.Cross(toPoint, Vector3.Up));
                        var normal = Vector3.Cross(toSide, toPoint);
                        return normal;
                    });
                    vertices[x + width * z].Normal = Vector3.Normalize(normals.Aggregate((a, b) => a + b));
                }
            }

            vertexBuffer = content.CreateVertexBuffer(vertices);

            /// indexing thanks to
            /// http://www.learnopengles.com/tag/triangle-strips/
            /// https://github.com/learnopengles/Learn-OpenGLES-Tutorials/blob/master/android/AndroidOpenGLESLessonsCpp/app/src/main/cpp/lesson8/HeightMap.cpp
            /// helped with the degenerate triangles. i'm really thankful
            var indices = new List<int>();
            for (var z = 0; z < length - 1; z++)
            {
                if (z > 0)
                {
                    // Degenerate begin: repeat first vertex
                    indices.Add(z * width);
                }

                for (var x = 0; x < width; x++)
                {
                    // One part of the strip
                    indices.Add((z * width) + x);
                    indices.Add(((z + 1) * width) + x);
                }

                if (z < length - 2)
                {
                    // Degenerate end: repeat last vertex
                    indices.Add(((z + 1) * width) + (length - 1));
                }
            }
            var indexArray = indices.ToArray();
            indexBuffer = content.CreateIndexBuffer(indexArray);
        }

        public float this[float x, float z]
        {
            get
            {
                // heightmap returns 0 outside of range. non-repeating or similar. that should be done through multiple chunks
                var ix = (int)Math.Floor(x * width);
                var iz = (int)Math.Floor(z * length);
                if (ix < 0 || iz < 0 || ix >= width || iz >= length)
                {
                    return 0;
                }
                return pointmap[ix, iz].Y;
            }
        }

        public Vector3[] GetHeightsAround1D(float x, float z, float range)
        {
            // convert from unit cube to integer array scale
            x *= width;
            z *= length;
            range *= Math.Min(width, length);

            var izmin = Math.Max(0, (int)Math.Floor(z - range));
            var izmax = Math.Min(length - 1, (int)Math.Ceiling(z + range));
            var diz = izmax - izmin;

            var ixmin = Math.Max(0, (int)Math.Floor(x - range));
            var ixmax = Math.Min(width - 1, (int)Math.Ceiling(x + range));
            var dix = ixmax - ixmin;

            if (dix <= 0 || diz <= 0)
            {
                return null;
            }

            var points = new Vector3[dix * diz];

            // ugly loop
            var j = 0;
            for (var iz = izmin; iz < izmax; iz++)
            {
                var i = 0;
                for (var ix = ixmin; ix < ixmax; ix++)
                {
                    points[i + dix * j] = pointmap[ix, iz];

                    i++;
                }
                j++;
            }

            return points;
        }

        public Vector3[,] GetHeightsAround2D(float x, float z, float range, bool includingSurroundingHeights)
        {
            // convert from unit cube to integer array scale
            x *= width;
            z *= length;
            range *= Math.Min(width, length);

            var shift = (includingSurroundingHeights ? 1 : 0);

            var izmin = Math.Max(0, (int)Math.Floor(z - range) - shift);
            var izmax = Math.Min(length - 1, (int)Math.Ceiling(z + range) + shift);
            var diz = izmax - izmin;

            var ixmin = Math.Max(0, (int)Math.Floor(x - range) - shift);
            var ixmax = Math.Min(width - 1, (int)Math.Ceiling(x + range) + shift);
            var dix = ixmax - ixmin;

            if (dix <= 0 || diz <= 0)
            {
                return null;
            }

            var points = new Vector3[dix, diz];

            // ugly loop
            var j = 0;
            for (var iz = izmin; iz < izmax; iz++)
            {
                var i = 0;
                for (var ix = ixmin; ix < ixmax; ix++)
                {
                    points[i, j] = pointmap[ix, iz];

                    i++;
                }
                j++;
            }

            return points;
        }
    }
}