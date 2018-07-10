using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Landscape : Object3D
    {
        private readonly string name;
        private readonly Image<Rgba32> map;
        private readonly int width;
        private readonly int height;
        private readonly int length;
        private readonly Vector3[,] pointmap;
        private readonly VertexBuffer vertexBuffer;
        private readonly IndexBuffer indexBuffer;

        public Vector3[,] Points => pointmap;
        public VertexBuffer VertexBuffer => vertexBuffer;
        public IndexBuffer IndexBuffer => indexBuffer;

        public Landscape(string name, GraphicsDevice graphicsDevice, Image<Rgba32> map)
        {
            this.name = name;
            this.map = map;
            this.width = map.Width;
            this.length = map.Height;
            this.height = Math.Min(width, length) / 8;
            this.pointmap = new Vector3[width, length];
            var vertices = new VertexPositionNormalTexture[width * length];

            for(var z = 0; z < length; z++)
            {
                for(var x = 0; x < width; x++)
                {
                    var h = map[x, z].ToScaledVector4().X * height;
                    pointmap[x, z] = new Vector3(1f * x / width, 1f * h / Math.Min(width, length), 1f * z / length);

                    var dx = map[Math.Max(0, x - 1), z].ToScaledVector4().X * height - map[Math.Min(width - 1, x + 1), z].ToScaledVector4().X * height;
                    var dz = map[x, Math.Max(0, z - 1)].ToScaledVector4().X * height - map[x, Math.Min(length - 1, z + 1)].ToScaledVector4().X * height;

                    var dxVector = new Vector3(2, dx, 0);
                    var dyVector = new Vector3(0, dz, 2);
                    var normal = Vector3.Cross(dyVector, dxVector);
                    normal.Normalize();

                    /// TODO
                    /// proper normal one day..
                    /// and proper texcoords
                    vertices[x + width * z] = new VertexPositionNormalTexture(pointmap[x, z], normal, (4f / width) * new Vector2(1f * x / width, 1f * z / length));
                }
            }

            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            var widthPlusOne = width + 1;

            var indexList = new short[(widthPlusOne + 1) * 2];
            foreach (var i in Enumerable.Range(0, indexList.Length))
            {
                if (i % 2 == 0)
                {
                    indexList[i] = (short)(i + widthPlusOne);
                }
                else
                {
                    indexList[i] = (short)(i - 1);
                }
            }
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexList.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indexList);
        }

        public float this[float x, float z]
        {
            get
            {
                // heightmap returns 0 outside of range. non-repeating or similar. that should be done through multiple chunks
                var ix = (int)Math.Floor(x * width);
                var iz = (int)Math.Floor(z * length);
                if(ix < 0 || iz < 0 || ix >= width || iz >= length)
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

            if(dix <= 0 || diz <= 0)
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



        void DrawFirst(BasicEffect b, Color color, Texture2D texture)
        {
            b.LightingEnabled = false;
            b.PreferPerPixelLighting = true;
            b.TextureEnabled = texture != null;
            b.VertexColorEnabled = false;
            b.Texture = texture;
            b.GraphicsDevice.SamplerStates[0] = new SamplerState
            {
                MaxMipLevel = 8,
                MipMapLevelOfDetailBias = -.5f,
                MaxAnisotropy = 8,
                Filter = TextureFilter.Anisotropic
            };

            var diffuseColor = b.DiffuseColor;
            b.DiffuseColor = color.ToVector3();

            b.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            b.GraphicsDevice.Indices = indexBuffer;

            var world = b.World;

            var widthPlusOne = width + 1;

            b.World *= Matrix.CreateScale(Radius) * Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
            foreach (var pass in b.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (var i in Enumerable.Range(0, widthPlusOne - 1))
                {
                    //b.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, Grid, i * DataWidth, DataWidth * 2, Indicees, 0, DataWidth - 1);

                    b.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, i * widthPlusOne, 0, widthPlusOne - 1);
                }
            }

            b.World = world;

            b.DiffuseColor = diffuseColor;
        }

        public void Draw(BasicEffect b, Color color, Texture2D texture)
        {

            b.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            b.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            b.GraphicsDevice.BlendState = BlendState.Opaque;
            DrawFirst(b, color, texture);
        }
    }
}