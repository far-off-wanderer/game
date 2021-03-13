using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Terrain : Object3D
    {

        int dataWidth;
        Vector3 size;
        VertexBuffer vertices;
        IndexBuffer indicees;
        List<Collider> colliders;
        //InfiniteTerrainDistanceField distanceField;

        public int DataWidth => dataWidth;
        public Vector3 Size => size;
        public IEnumerable<Collider> Colliders => colliders;
        //public InfiniteTerrainDistanceField DistanceField => distanceField;

        public Terrain(Vector3 position, Vector3 size)
        {
            this.Position = position;

            this.dataWidth = 0;
            this.size = size;
            this.vertices = null;
            this.indicees = null;
            this.colliders = null;
            //this.distanceField = null;
        }

        public void LoadFromTexture2D(Texture2D TerrainTexture, Environment Environment)
        {
            if (TerrainTexture.Width != TerrainTexture.Height)
            {
                throw new Exception("Terrain has to be Square in Size");
            }
            this.dataWidth = TerrainTexture.Width + 1;
            this.colliders = new List<Collider>();

            var colorWidth = DataWidth - 1;

            var heightData = new byte[DataWidth * DataWidth];
            var displaceData = new Vector3[DataWidth * DataWidth];
            var displaceTextureData = new Vector2[DataWidth * DataWidth];

            var colorData = new Color[colorWidth * colorWidth];
            TerrainTexture.GetData(colorData);

            for (var y = 0; y < DataWidth; y++)
            {
                for (var x = 0; x < DataWidth; x++)
                {
                    var sample = colorData[(x % colorWidth) + (y % colorWidth) * colorWidth];
                    heightData[x + y * DataWidth] = sample.R;

                    if (x < colorWidth && y < colorWidth)
                    {
                        displaceData[x + DataWidth * y] = Noise.Vector3.Get(x / 4, y / 4);
                        displaceTextureData[x + DataWidth * y] = Noise.Vector2.Get(x / 4, y / 4, shift: 3);
                    }
                    else
                    {
                        displaceData[x + DataWidth * y] = displaceData[(x % colorWidth) + (y % colorWidth) * DataWidth];
                        displaceTextureData[x + DataWidth * y] = displaceTextureData[(x % colorWidth) + (y % colorWidth) * DataWidth];
                    }

                    displaceTextureData[x + DataWidth * y] =
                        -Vector2.UnitX * displaceData[x + DataWidth * y].X
                        +
                        -Vector2.UnitY * displaceData[x + DataWidth * y].Z;
                }
            }

            var grid = new VertexPositionNormalTexture[DataWidth * DataWidth];

            var halfWidthOfSmallerStepSize = Math.Min(Size.X, Size.Z) / DataWidth / 1.414f;

            //var hasCache = InfiniteTerrainDistanceField.HasCache;

            foreach (var y in Enumerable.Range(0, DataWidth))
            {
                foreach (var x in Enumerable.Range(0, DataWidth))
                {
                    var Height = heightData[x + DataWidth * y];
                    var Point = new Vector3(x / (float)(DataWidth - 1), Height / 256f, y / (float)(DataWidth - 1));
                    Point = 2 * Point - new Vector3(1, 1, 1);

                    Point = Point * Size / 2 + Position;

                    var c = Height / 255f;
                    var color = new Color();
                    if (c < 0.02f)
                    {
                        color = Color.DarkBlue;
                    }



                    Point += displaceData[x + DataWidth * y] * new Vector3(1, 1.41f, 1) * halfWidthOfSmallerStepSize * (float)Math.Pow(c, 0.1f);

                    var dx =
                        (
                            heightData[((x + DataWidth - 1) % (DataWidth - 1)) + DataWidth * y]
                            -
                            heightData[((x + 1) % (DataWidth - 1)) + DataWidth * y]
                        ) / 256f;
                    var dy =
                        (
                            heightData[x + DataWidth * ((y + DataWidth - 1) % (DataWidth - 1))]
                            -
                            heightData[x + DataWidth * ((y + 1) % (DataWidth - 1))]
                        ) / 256f;

                    var dxVector = new Vector3(Size.X * 2 / DataWidth, dx * Size.Y, 0);
                    var dyVector = new Vector3(0, dy * Size.Y, Size.Z * 2 / DataWidth);
                    var normal = Vector3.Cross(dyVector, dxVector);
                    normal.Normalize();
                    var light = new Vector3(1, 1, 0);
                    light.Normalize();
                    var shade = Vector3.Dot(normal, light);
                    var snowColor = new Vector3(1, 1, 1);
                    var grassColor = new Vector3(0.1f, 0.4f, 0.01f);
                    //grassColor = new Vector3(0.01f, 0.01f, 0.01f);
                    {
                        c -= 0.7f;
                        c *= 2f;
                        if (c < 0) c = 0;
                        else c *= 2;
                        if (c > 1) c = 1;
                        color = new Color(shade * MathHelper.Lerp(grassColor.X, snowColor.X, c), shade * MathHelper.Lerp(grassColor.Y, snowColor.Y, c), shade * MathHelper.Lerp(grassColor.Z, snowColor.Z, c));
                    }
                    var h = Height / 255f;
                    if (h < 0.03f)
                    {
                        var blend = h;
                        blend = blend / 0.03f;
                        var clr1 = new Color(0, 0, 0.2f).ToVector3();
                        var clr2 = color.ToVector3();
                        var clr = (1 - blend) * clr1 + blend * clr2;
                        color = new Color(clr.X, clr.Y, clr.Z);
                    }

                    color *= 1.3f;
                    var texcoord = new Vector2(0.5f * Point.X / Size.X + 0.5f, 0.5f * Point.Z / Size.Z + 0.5f);
                    texcoord += 0.3f * displaceTextureData[x + DataWidth * y] / (Math.Max(TerrainTexture.Width, TerrainTexture.Height) * 2 * 1.414f);

                    Point += (2 * Height / 255f - 1) * normal * new Vector3(1, Height / 255f, 1) * 2500;

                    var position = Point + Position - halfWidthOfSmallerStepSize * Vector3.Up;

                    //if (hasCache) // optimizing for now
                    //{
                    //    if (Math.Abs(position.Y) <= halfWidthOfSmallerStepSize)
                    //    {
                    //        colliders.Add(new Collider(position, halfWidthOfSmallerStepSize));
                    //    }
                    //}
                    //else
                    //{
                    //    colliders.Add(new Collider(position, halfWidthOfSmallerStepSize));
                    //}

                    color = Color.White * shade;

                    grid[x + DataWidth * y] = new VertexPositionNormalTexture(Point, normal, texcoord * 64);
                }
            }

            // optimizing.. for now
            //Environment.StaticColliders = hasCache ? Colliders.ToArray() : Colliders.Where(c => Math.Abs(c.Position.Y) <= halfWidthOfSmallerStepSize).ToArray();

            vertices = new VertexBuffer(TerrainTexture.GraphicsDevice, typeof(VertexPositionNormalTexture), grid.Length, BufferUsage.WriteOnly);
            vertices.SetData(grid);

            var indexList = new short[(DataWidth + 1) * 2];
            foreach (var i in Enumerable.Range(0, indexList.Length))
            {
                if (i % 2 == 0)
                {
                    indexList[i] = (short)(i + DataWidth);
                }
                else
                {
                    indexList[i] = (short)(i - 1);
                }
            }

            indicees = new IndexBuffer(TerrainTexture.GraphicsDevice, IndexElementSize.SixteenBits, indexList.Length, BufferUsage.WriteOnly);
            indicees.SetData(indexList);

            //this.distanceField = new InfiniteTerrainDistanceField(this);

            //Environment.DistanceField = DistanceField;
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

            b.GraphicsDevice.SetVertexBuffer(vertices);
            b.GraphicsDevice.Indices = indicees;

            var world = b.World;

            b.World *= Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
            foreach (var pass in b.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (var i in Enumerable.Range(0, DataWidth - 1))
                {
                    //b.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, Grid, i * DataWidth, DataWidth * 2, Indicees, 0, DataWidth - 1);

                    b.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, i * DataWidth, 0, DataWidth - 1);
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
