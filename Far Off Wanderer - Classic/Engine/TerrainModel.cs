using Far_Off_Wanderer___Classic.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conesoft.Engine
{
    public class TerrainModel
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }

        public int DataWidth { get; private set; }
        public byte[] HeightData { get; private set; }
        public Vector3[] DisplaceData { get; private set; }
        public Vector2[] DisplaceTextureData { get; private set; }
        public VertexPositionColorTexture[] Grid { get; private set; }
        public short[] Indicees { get; private set; }
        public List<Collider> Colliders { get; private set; }

        VertexBuffer vertices;
        IndexBuffer indicees;

        public TerrainModel()
        {
            DataWidth = 0;
            HeightData = null;
            Colliders = new List<Collider>();
        }

        public void LoadFromTexture2D(Texture2D TerrainTexture, Conesoft.Game.DefaultEnvironment Environment)
        {
            if (TerrainTexture.Width != TerrainTexture.Height)
            {
                throw new Exception("Terrain has to be Square in Size");
            }
            DataWidth = TerrainTexture.Width + 1;
            var colorWidth = DataWidth - 1;

            HeightData = new byte[DataWidth * DataWidth];
            DisplaceData = new Vector3[DataWidth * DataWidth];
            DisplaceTextureData = new Vector2[DataWidth * DataWidth];

            var colorData = new Color[colorWidth * colorWidth];
            TerrainTexture.GetData(colorData);

            for (var y = 0; y < DataWidth; y++)
            {
                for (var x = 0; x < DataWidth; x++)
                {
                    var sample = colorData[(x % colorWidth) + (y % colorWidth) * colorWidth];
                    //sample.R = (byte)(SimplexNoise.Singleton.MultiNoise01(2, 1f * (x % colorWidth) / (colorWidth / 8), 1f * (y % colorWidth) / (colorWidth / 8)) * 100);
                    HeightData[x + y * DataWidth] = sample.R;

                    if (x < colorWidth && y < colorWidth)
                    {
                        DisplaceData[x + DataWidth * y] = Environment.RandomPointInUnitSphere();
                        DisplaceTextureData[x + DataWidth * y] = Environment.RandomPointInUnitCircle();
                    }
                    else
                    {
                        DisplaceData[x + DataWidth * y] = DisplaceData[(x % colorWidth) + (y % colorWidth) * DataWidth];
                        DisplaceTextureData[x + DataWidth * y] = DisplaceTextureData[(x % colorWidth) + (y % colorWidth) * DataWidth];
                    }

                    DisplaceTextureData[x + DataWidth * y] =
                        -Vector2.UnitX * DisplaceData[x + DataWidth * y].X
                        +
                        -Vector2.UnitY * DisplaceData[x + DataWidth * y].Z;
                }
            }

            Grid = new VertexPositionColorTexture[DataWidth * DataWidth];
            foreach (var y in Enumerable.Range(0, DataWidth))
            {
                foreach (var x in Enumerable.Range(0, DataWidth))
                {
                    var Height = HeightData[x + DataWidth * y];
                    var Point = new Vector3(x / (float)(DataWidth - 1), Height / 256f, y / (float)(DataWidth - 1));
                    Point = 2 * Point - new Vector3(1, 1, 1);

                    Point = Point * Size / 2 + Position;

                    var halfWidthOfSmallerStepSize = Math.Min(Size.X, Size.Z) / DataWidth / 1.414f;

                    var c = Height / 255f;
                    var color = new Color();
                    if (c < 0.02f)
                    {
                        color = Color.DarkBlue;
                    }



                    Point += DisplaceData[x + DataWidth * y] * new Vector3(1, 1.41f, 1) * halfWidthOfSmallerStepSize * (float)Math.Pow(c, 0.1f);

                    var dx =
                        (
                            HeightData[((x + DataWidth - 1) % (DataWidth - 1)) + DataWidth * y]
                            -
                            HeightData[((x + 1) % (DataWidth - 1)) + DataWidth * y]
                        ) / 256f;
                    var dy =
                        (
                            HeightData[x + DataWidth * ((y + DataWidth - 1) % (DataWidth - 1))]
                            -
                            HeightData[x + DataWidth * ((y + 1) % (DataWidth - 1))]
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
                    texcoord += 0.3f * DisplaceTextureData[x + DataWidth * y] / (Math.Max(TerrainTexture.Width, TerrainTexture.Height) * 2 * 1.414f);

                    Point += (2 * Height / 255f - 1) * normal * new Vector3(1, Height / 255f, 1) * 2500;

                    var position = Point + Position - halfWidthOfSmallerStepSize * Vector3.Up;
                    if (Math.Abs(position.Y) <= halfWidthOfSmallerStepSize)
                    {
                        Colliders.Add(new Collider(position, halfWidthOfSmallerStepSize));
                    }

                    Grid[x + DataWidth * y] = new VertexPositionColorTexture(Point, color, texcoord * 64);
                }
            }

            Environment.StaticColliders = Colliders;

            vertices = new VertexBuffer(TerrainTexture.GraphicsDevice, typeof(VertexPositionColorTexture), Grid.Length, BufferUsage.WriteOnly);
            vertices.SetData(Grid);

            Indicees = new short[(DataWidth + 1) * 2];
            foreach (var i in Enumerable.Range(0, Indicees.Length))
            {
                if (i % 2 == 0)
                {
                    Indicees[i] = (short)(i + DataWidth);
                }
                else
                {
                    Indicees[i] = (short)(i - 1);
                }
            }

            indicees = new IndexBuffer(TerrainTexture.GraphicsDevice, IndexElementSize.SixteenBits, Indicees.Length, BufferUsage.WriteOnly);
            indicees.SetData(Indicees);
        }

        public void DrawFirst(BasicEffect b, Texture2D texture = null)
        {
            b.LightingEnabled = false;
            b.TextureEnabled = texture != null;
            b.VertexColorEnabled = true;
            b.Texture = texture;
            b.GraphicsDevice.SamplerStates[0] = new SamplerState
            {
                MaxMipLevel = 8,
                MipMapLevelOfDetailBias = -.5f,
                MaxAnisotropy = 8,
                Filter = TextureFilter.Anisotropic
            };

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
        }

        public void Draw(BasicEffect b, Texture2D texture = null)
        {

            b.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            b.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            b.GraphicsDevice.BlendState = BlendState.Opaque;
            DrawFirst(b, texture);
        }
    }
}
