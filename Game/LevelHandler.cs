﻿using Far_Off_Wanderer.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XNA = Microsoft.Xna.Framework.Input;

namespace Far_Off_Wanderer
{
    public class LevelHandler : Scenes.Handler<Scenes.Level>
    {
        public class InputActions
        {
            readonly Input input;
            public InputActions(Input input) => this.input = input;

            public bool CancelingLevel => input.Keyboard.On[(int)Keys.Escape] || input.TouchKeys.OnBackButton;
            public bool CheckingForExitClickAfterGameOver => input.Keyboard.On[(int)Keys.Space] || input.GamePad.On[Buttons.A];
            public bool ToggleWireframe => input.Keyboard.On[(int)Keys.F2];
            public bool Toggling3dDisplay => input.Keyboard.On[(int)Keys.F3];
            public bool TogglingOverUnder => input.Keyboard.On[(int)Keys.F4];
            public bool ZoomingIn => input.GamePad.While[Buttons.DPadUp];
            // TODO: Add joysticks to Input
            public float CameraYaw => XNA.GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X;
            public float CameraPitch => XNA.GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y;

            public bool TurningLeft => input.Keyboard.While[(int)Keys.A] || input.Keyboard.While[(int)Keys.Left];
            public bool TurningRight => input.Keyboard.While[(int)Keys.D] || input.Keyboard.While[(int)Keys.Right];
            public bool TurningUp => false;
            public bool TurningDown => false;
            public bool Accelerating => input.Keyboard.While[(int)Keys.W] || input.Keyboard.While[(int)Keys.Up]; // input.Keyboard.While[(int)Keys.LeftShift] || input.Keyboard.While[(int)Keys.RightShift];
            public bool Decelerating => input.Keyboard.While[(int)Keys.S] || input.Keyboard.While[(int)Keys.Down]; // input.Keyboard.While[(int)Keys.LeftControl] || input.Keyboard.While[(int)Keys.RightControl];
            public bool Shooting => input.Keyboard.While[(int)Keys.Space];
            public bool StrafingLeft => input.Keyboard.On[(int)Keys.Q];
            public bool StrafingRight => input.Keyboard.On[(int)Keys.E];

            public bool WinQuicklyCheat => input.Keyboard.On[(int)Keys.O];
        }

        struct State<T>
        {
            private T previousValue;
            private T currentValue;

            public State(T value)
            {
                previousValue = value;
                currentValue = value;
            }

            public void Set(T value)
            {
                previousValue = currentValue;
                currentValue = value;
            }

            public void Reset(T value)
            {
                previousValue = value;
                currentValue = value;
            }

            public T Value => currentValue;

            public bool HasChanged => previousValue.Equals(currentValue) == false;
        }

        static class State
        {
            public static State<T> Track<T>(T value) => new(value);
        }

        public LevelHandler(Scenes.Level scene)
        {
            var dead = State.Track(false);
            var won = State.Track(false);
            var started = false;
            var fadeOut = default(float?);
            var fadeIn = 0f;
            var fadeInTime = 0.25f;
            var runtime = default(TimeSpan);
            var environment = new Environment()
            {
                Random = new Random()
            };
            var level = default(Level);

            var models = default(Dictionary<string, Model>);
            var textures = default(Dictionary<string, Texture2D>);
            var spriteFonts = default(Dictionary<string, SpriteFont>);

            var wireframe = false;
            var runIn3d = false;
            var overUnder = true;
            var leftEye = default(RenderTarget2D);
            var rightEye = default(RenderTarget2D);

            void StartGame()
            {
                runtime = TimeSpan.Zero;
                dead.Reset(false);
                won.Reset(false);
                fadeOut = null;

                fadeInTime = 0.25f;

                started = true;
            }

            void UpdateFadeout(TimeSpan e)
            {
                if (fadeOut.HasValue == false)
                {
                    fadeOut = 0;
                }
                else
                {
                    fadeOut += (float)e.TotalSeconds / 3;
                    if (fadeOut > 1)
                    {
                        fadeOut = 1;
                    }
                }
            }

            Begin = async content =>
            {
                // at start for loading screen
                spriteFonts = content.GetAll<SpriteFont>(Data.Font);

                // delayed till game starts
                await Task.Run(() =>
                {
                    BoundingSphere CreateMerged(IEnumerable<BoundingSphere> spheres) => spheres.Aggregate((a, b) => BoundingSphere.CreateMerged(a, b));

                    models = content.GetAll<Model>(Data.Ship, Data.Drone, Data.Spaceship);
                    environment.Sounds = content.GetAll<SoundEffect>("puiiw", "explosion");
                    textures = content.GetAll<Texture2D>("vignette", "Floor", "arrow", "dot", "LDR_LLL1_0", "Shadow", Data.BlackBackground, Data.Bullet, Data.GameOverOverlay, Data.GameWonOverlay, Data.Grass, Data.Sparkle);

                    var landscapes = scene.Surfaces.SelectFromDictionary((name, surface, index) =>
                    {
                        return new Landscape(name, content, content.Get<Image<Rgba32>>(name), surface.Noise.Bottom, surface.Noise.Top, surface.Color, surface.BorderToInfinity)
                        {
                            Radius = surface.Size,
                            Position = Vector3.UnitX * -surface.Size / 2 + surface.Position + Vector3.UnitZ * -surface.Size / 2
                        };
                    });

                    var boundaries = models.SelectAsDictionary(m => CreateMerged(m.Meshes.Select(mesh => mesh.BoundingSphere)).Radius);

                    level = new Level(environment, boundaries, landscapes);
                });

                StartGame();
            };

            Update = (timeSpan, input) =>
            {
                var actions = new InputActions(input);

                if (started)
                {
                    environment.Update(timeSpan);

                    if (actions.CancelingLevel)
                    {
                        OnNext(scene.On.Cancel);
                    }

                    if (actions.ToggleWireframe)
                    {
                        wireframe = !wireframe;
                    }

                    if (actions.Toggling3dDisplay)
                    {
                        runIn3d = !runIn3d;
                    }

                    if (actions.TogglingOverUnder)
                    {
                        overUnder = !overUnder;
                    }

                    void OnGameOver()
                    {
                        OnNext(won.Value ? scene.On.Won : scene.On.GameOver);
                    }

                    environment.ActiveCamera = level.Camera;
                    environment.Flipped = false; // Orientation == PageOrientation.LandscapeRight;
                    environment.Actions = actions;

                    runtime += timeSpan;

                    fadeIn = Math.Min((float)runtime.TotalSeconds / fadeInTime, 1);

                    level.UpdateScene(
                        runtime.TotalSeconds > fadeInTime ? timeSpan : TimeSpan.Zero,
                        actions
                    );

                    if (!won.Value && !dead.Value)
                    {
                        dead.Set(!won.Value && level.NoLocalPlayerLeft);
                        won.Set(actions.WinQuicklyCheat || (!dead.Value && level.NoEnemiesLeft));
                    }


                    if (actions.WinQuicklyCheat)
                    {
                        var x = won.Value;
                    }

                    if (dead.HasChanged)
                    {
                        //environment.Sounds[Data.GameOverSound].Play();
                    }

                    if (won.HasChanged)
                    {
                        //environment.Sounds[Data.GoodSound].Play();
                        //UpdateHighscore();
                    }

                    if (won.Value || dead.Value)
                    {
                        if (actions.CheckingForExitClickAfterGameOver)
                        {
                            OnGameOver();
                        };
                        UpdateFadeout(timeSpan);
                    }
                }
            };

            Draw = graphics =>
            {
                var g = graphics.GraphicsDevice;
                var s = graphics.SpriteBatch;

                environment.ScreenSize = new System.Drawing.Size(g.Viewport.Width, g.Viewport.Height);

                leftEye ??= new RenderTarget2D(
                    g,
                    g.DisplayMode.Width,
                    g.DisplayMode.Height,
                    false,
                    g.PresentationParameters.BackBufferFormat,
                    g.PresentationParameters.DepthStencilFormat,
                    g.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );
                rightEye ??= new RenderTarget2D(
                    g,
                    g.DisplayMode.Width,
                    g.DisplayMode.Height,
                    false,
                    g.PresentationParameters.BackBufferFormat,
                    g.PresentationParameters.DepthStencilFormat,
                    g.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );

                var views = runIn3d ? new[]
                {
                    (
                        target: leftEye,
                        area: overUnder ? new Microsoft.Xna.Framework.Rectangle(0, 0, g.Viewport.Width, g.Viewport.Height / 2) : new Microsoft.Xna.Framework.Rectangle(0, 0, g.Viewport.Width / 2, g.Viewport.Height),
                        eye: -30f
                    ),
                    (
                        target: rightEye,
                        area: overUnder ? new Microsoft.Xna.Framework.Rectangle(0, g.Viewport.Height - g.Viewport.Height / 2, g.Viewport.Width, g.Viewport.Height / 2) : new Microsoft.Xna.Framework.Rectangle(g.Viewport.Width - g.Viewport.Width / 2, 0, g.Viewport.Width / 2, g.Viewport.Height),
                        eye: 30f
                    )
                } : new[]
                {
                    (
                        target: leftEye,
                        area: new Microsoft.Xna.Framework.Rectangle(0, 0, g.Viewport.Width, g.Viewport.Height),
                        eye: 0f
                    )
                };
                foreach (var (target, area, eye) in views)
                {
                    g.SetRenderTarget(target);

                    if (!started)
                    {
                        g.Clear(Microsoft.Xna.Framework.Color.Black);
                        var text = "loading...";
                        var textSize = spriteFonts[Data.Font].MeasureString(text);
                        s.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                        s.DrawString(spriteFonts[Data.Font], "loading...", (new Vector2(g.Viewport.Width, g.Viewport.Height) - textSize) / 2, Microsoft.Xna.Framework.Color.White);
                        s.End();
                    }

                    if (started)
                    {
                        level.Camera.FarCutOff = scene.Surfaces.First(s => s.Value.BorderToInfinity.HasValue).Value.Size;
                        var camera = new CameraModel(level.Camera, eye, new System.Drawing.Size(g.Viewport.Width, g.Viewport.Height));

                        var basicEffect = new BasicEffect(g);

                        var backgroundColor = scene.Environment.BackgroundColor;

                        g.Clear(backgroundColor);

                        g.BlendState = BlendState.Opaque;
                        g.DepthStencilState = DepthStencilState.Default;

                        basicEffect.EnableDefaultLighting();

                        basicEffect.LightingEnabled = true;
                        basicEffect.TextureEnabled = true;
                        basicEffect.VertexColorEnabled = false;
                        basicEffect.FogEnabled = true;
                        basicEffect.FogColor = backgroundColor.ToVector3();
                        basicEffect.FogStart = MathHelper.Lerp(20000f, 10f, scene.Environment.Fog);
                        basicEffect.FogEnd = scene.Surfaces.First(s => s.Value.BorderToInfinity.HasValue).Value.Size;

                        basicEffect.DirectionalLight0.Enabled = true;
                        basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0, 0, 0);
                        basicEffect.DirectionalLight0.Direction = new Vector3(1, 0, 0);
                        basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0, 0);

                        basicEffect.PreferPerPixelLighting = true;

                        basicEffect.World = Matrix.Identity;
                        basicEffect.View = camera.View;
                        basicEffect.Projection = camera.Projection;

                        g.BlendState = BlendState.Opaque;

                        void DrawSpaceships()
                        {
                            foreach (var spaceship in level.Objects3D.OfType<Spaceship>())
                            {
                                var model = models[spaceship.Id];

                                g.RasterizerState = wireframe ? new RasterizerState()
                                {
                                    FillMode = FillMode.WireFrame,
                                    CullMode = CullMode.None
                                } : RasterizerState.CullNone;

                                model.Draw(Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateFromYawPitchRoll(spaceship.HorizontalOrientation, spaceship.VerticalOrientation, 0) * Matrix.CreateTranslation(spaceship.Position), camera.View, camera.Projection);
                            }
                        }
                        DrawSpaceships();

                        void DrawLandscapes()
                        {
                            g.RasterizerState = RasterizerState.CullNone;

                            basicEffect.GraphicsDevice.BlendState = BlendState.Opaque;

                            basicEffect.LightingEnabled = false;
                            basicEffect.PreferPerPixelLighting = true;
                            basicEffect.TextureEnabled = false;
                            basicEffect.VertexColorEnabled = false;

                            foreach (var landscape in level.Objects3D.OfType<Landscape>())
                            {
                                var diffuseColor = basicEffect.DiffuseColor;
                                basicEffect.DiffuseColor = landscape.Color.ToVector3();

                                basicEffect.GraphicsDevice.SetVertexBuffer(landscape.VertexBuffer);
                                basicEffect.GraphicsDevice.Indices = landscape.IndexBuffer;

                                var world = basicEffect.World;

                                basicEffect.World *= Matrix.CreateScale(landscape.Radius) * Matrix.CreateTranslation(landscape.Position);
                                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                                {
                                    pass.Apply();
                                    basicEffect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, landscape.IndexBuffer.IndexCount - 2);
                                }

                                basicEffect.World = world;
                                basicEffect.DiffuseColor = diffuseColor;
                            }
                        }
                        DrawLandscapes();

                        void DrawShadowsBelowSpaceships()
                        {
                            foreach (var landscape in level.Objects3D.OfType<Landscape>())
                            {
                                foreach (var spaceship in level.Objects3D.OfType<Spaceship>())
                                {
                                    void DrawShadow(BasicEffect b, Microsoft.Xna.Framework.Color color, Texture2D texture, Vector3[,] shadowPoints, Vector3 toObject, float sizeOfObject)
                                    {
                                        b.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                                        b.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                                        b.LightingEnabled = false;
                                        b.PreferPerPixelLighting = true;
                                        b.TextureEnabled = texture != null;
                                        b.VertexColorEnabled = true;
                                        b.Texture = texture;
                                        b.GraphicsDevice.SamplerStates[0] = new SamplerState
                                        {
                                            MaxMipLevel = 8,
                                            MipMapLevelOfDetailBias = -.5f,
                                            MaxAnisotropy = 8,
                                            Filter = TextureFilter.Anisotropic,
                                            AddressU = TextureAddressMode.Clamp,
                                            AddressV = TextureAddressMode.Clamp,
                                            AddressW = TextureAddressMode.Clamp
                                        };

                                        var diffuseColor = b.DiffuseColor;
                                        b.DiffuseColor = color.ToVector3();

                                        var world = b.World;

                                        b.World *= Matrix.CreateScale(landscape.Radius) * Matrix.CreateTranslation(landscape.Position.X, landscape.Position.Y, landscape.Position.Z);

                                        var shift = new Vector3(0, 0.0001f, 0);

                                        foreach (var pass in b.CurrentTechnique.Passes)
                                        {
                                            pass.Apply();
                                            var triangleStrip = new VertexPositionColorTexture[4];
                                            for (var i = 0; i < triangleStrip.Length; i++)
                                            {
                                                triangleStrip[i] = new VertexPositionColorTexture(Vector3.Zero, Microsoft.Xna.Framework.Color.White, Vector2.Zero);
                                            }
                                            for (var z = 0; z < shadowPoints.GetLength(1) - 1; z++)
                                            {
                                                for (var x = 0; x < shadowPoints.GetLength(0) - 1; x++)
                                                {
                                                    VertexPositionColorTexture ComputeShadowPoint(Vector3 at)
                                                    {
                                                        var pointToObject = (toObject - at) / sizeOfObject;
                                                        var textureSpace = pointToObject * .5f + Vector3.One * .5f;

                                                        var fade = 1 / (1 + pointToObject.Y * pointToObject.Y * (pointToObject.Y < 0 ? 1000000000000000 : 1));

                                                        fade = 1;

                                                        return new VertexPositionColorTexture(
                                                            position: at + shift,
                                                            color: new Microsoft.Xna.Framework.Color(Microsoft.Xna.Framework.Color.White, fade),
                                                            textureCoordinate: new Vector2(textureSpace.X, textureSpace.Z)
                                                        );
                                                    }
                                                    // verified through wireframe
                                                    triangleStrip[0] = ComputeShadowPoint(shadowPoints[x + 0, z + 0]);
                                                    triangleStrip[1] = ComputeShadowPoint(shadowPoints[x + 1, z + 0]);
                                                    triangleStrip[2] = ComputeShadowPoint(shadowPoints[x + 0, z + 1]);
                                                    triangleStrip[3] = ComputeShadowPoint(shadowPoints[x + 1, z + 1]);

                                                    b.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, triangleStrip, 0, 2);
                                                }
                                            }
                                        }

                                        b.World = world;
                                        b.DiffuseColor = diffuseColor;
                                    }

                                    var distanceTo = (spaceship.Position - landscape.Position) / landscape.Radius;
                                    var radius = spaceship.Radius / landscape.Radius;

                                    var factor = 4;

                                    var points = landscape.GetHeightsAround2D(distanceTo.X, distanceTo.Z, radius * factor, includingSurroundingHeights: true);

                                    if (points != null)
                                    {
                                        DrawShadow(basicEffect, scene.Environment.BackgroundColor.Complementary().GreyedOut(.8f) * .1f, textures["Shadow"], points, distanceTo, radius * factor);
                                    }
                                }
                            }
                        }
                        DrawShadowsBelowSpaceships();


                        void DrawSparclesFireballsAndBullets()
                        {
                            var bounds = g.Viewport.Bounds;
                            s.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                            g.DepthStencilState = DepthStencilState.DepthRead;
                            foreach (var explosion in level.Objects3D.OfType<Explosion>())
                            {
                                var transformed = g.Viewport.Project(explosion.Position, camera.Projection, camera.View, Matrix.Identity);
                                var distance = (explosion.Position - level.Camera.Position).Length();
                                if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                                {
                                    var sprite = textures[Data.Sparkle];
                                    var width = explosion.CurrentSize * Math.Max(bounds.Width, bounds.Height) / distance;
                                    width *= 4f;
                                    if (explosion.Id == Data.Fireball)
                                    {
                                        width *= 2f;
                                    }
                                    var rectangle = new Microsoft.Xna.Framework.Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                                    if (rectangle.Intersects(g.Viewport.Bounds))
                                    {
                                        s.Draw(sprite, rectangle, null, new Microsoft.Xna.Framework.Color(2 - explosion.Age, 2 - explosion.Age, 1 - explosion.Age / 2, 2 - explosion.Age), explosion.StartSpin + explosion.Spin * explosion.Age, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                                    }
                                }
                            }
                            s.End();
                            s.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                            g.DepthStencilState = DepthStencilState.DepthRead;
                            foreach (var bullet in level.Objects3D.OfType<Bullet>())
                            {
                                var transformed = g.Viewport.Project(bullet.Position, camera.Projection, camera.View, Matrix.Identity);
                                var distance = (bullet.Position - level.Camera.Position).Length();
                                if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                                {
                                    var sprite = textures[bullet.Id];
                                    var width = Math.Max(bounds.Width, bounds.Height) * bullet.Radius / distance;
                                    width *= 8f;
                                    var rectangle = new Microsoft.Xna.Framework.Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                                    if (rectangle.Intersects(g.Viewport.Bounds))
                                    {
                                        s.Draw(sprite, rectangle, null, Microsoft.Xna.Framework.Color.White, 0, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                                    }
                                }
                            }
                            s.End();
                        }
                        DrawSparclesFireballsAndBullets();

                        void DrawText()
                        {
                            var enemyCount = level.Objects3D.OfType<Spaceship>().Count();
                            var enemyCountText = $"Enemies:  {(enemyCount > 0 ? enemyCount - 1 : 0):00}";
                            var enemyCountSize = spriteFonts[Data.Font].MeasureString(enemyCountText).X;

                            var objectCount = level.Objects3D.Count();
                            var objectCountText = objectCount.ToString();
                            var objectCountSize = spriteFonts[Data.Font].MeasureString(objectCountText).X;

                            var playerVerticalRotation = $"vertical Rotation: {level.LocalPlayer.ControlledObject.VerticalOrientation}";
                            var playerVerticalRotationLength = spriteFonts[Data.Font].MeasureString(playerVerticalRotation).X;

                            var osdBlend = Microsoft.Xna.Framework.Color.White * (1f - MathHelper.Clamp((fadeOut ?? 0) * 1.5f - 1, 0, 1));

                            s.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                            s.DrawString(spriteFonts[Data.Font], enemyCountText, new Vector2(g.Viewport.Width - enemyCountSize - 20, 10), osdBlend);

                       //     s.DrawString(spriteFonts[Data.Font], objectCountText, new Vector2(g.Viewport.Width - objectCountSize - 20, 110), osdBlend);

                       //     s.DrawString(spriteFonts[Data.Font], playerVerticalRotation, new Vector2(g.Viewport.Width - playerVerticalRotationLength - 20, 210), osdBlend);

                            if (fadeOut.HasValue)
                            {
                                s.Draw(textures[Data.BlackBackground], new Microsoft.Xna.Framework.Rectangle(0, 0, g.Viewport.Width, g.Viewport.Height), new Microsoft.Xna.Framework.Color(0, 0, 0, fadeOut.Value / 2));

                                var splashTexture = textures[won.Value ? Data.GameWonOverlay : Data.GameOverOverlay];
                                var (Width, Height) = (1280, 768);

                                var screen = g.Viewport;
                                var screenAspect = (float)screen.Width / screen.Height;
                                var output = new Microsoft.Xna.Framework.Rectangle();
                                var titleAspect = (float)Width / Height;
                                {
                                    output.Width = Width * screen.Height / Height;
                                    output.Height = screen.Height;
                                    output.X = -(output.Width - screen.Width) / 2;
                                    output.Y = 0;
                                }
                                if (dead.Value || won.Value)
                                {
                                    s.Draw(splashTexture, output, new Microsoft.Xna.Framework.Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                                }
                            }
                            s.End();
                        }
                        DrawText();


                        void DrawMap()
                        {
                            float Mod(float a, float b)
                            {
                                return a - b * (float)Math.Floor(a / b);
                            }
                            var range = scene.Surfaces.First(s => s.Value.BorderToInfinity.HasValue).Value.Size;
                            s.Begin();
                            var floorSize = g.Viewport.Width / 4;
                            var maparea = new Microsoft.Xna.Framework.Rectangle((int)(g.Viewport.Width - floorSize * 1.1f), (int)(g.Viewport.Height - floorSize * 1.1f), floorSize, floorSize);
                            foreach (var ship in level.Objects3D.OfType<Spaceship>().OrderBy(s => s == level.LocalPlayer.ControlledObject))
                            {
                                var shipicon = ship.Id == Data.Ship ? textures["arrow"] : textures["dot"];
                                var shipsize = (1 / 8f) * (maparea.Width + maparea.Height) / 2f;
                                var shipcolor = level.LocalPlayer.ControlledObject == ship ? Microsoft.Xna.Framework.Color.White : Microsoft.Xna.Framework.Color.Red;
                                var shipposition = new Vector2(
                                    MathHelper.Lerp(maparea.Left, maparea.Right, Mod(ship.Position.X - range / 2f, range) / range),
                                    MathHelper.Lerp(maparea.Top, maparea.Bottom, Mod(ship.Position.Z - range / 2f, range) / range)
                                );
                                s.Draw(shipicon, shipposition, null, shipcolor, (float)Math.Atan2(ship.Forward.X, -ship.Forward.Z), Vector2.One * shipicon.Width / 2, shipsize / shipicon.Width, SpriteEffects.None, 0f);
                            }
                            s.End();

                            //s.Begin();
                            //s.Draw(floor, maparea, new Color(255, 255, 255, 4));
                            //s.End();
                        }
                        DrawMap();

                        void DrawCameraEffects()
                        {
                            s.Begin(blendState: BlendState.Additive);
                            var noise = textures["LDR_LLL1_0"];
                            for (var y = environment.Random.Next(1 - noise.Height, 0); y < g.Viewport.Height; y += noise.Height)
                            {
                                for (var x = environment.Random.Next(1 - noise.Width, 0); x < g.Viewport.Width; x += noise.Width)
                                {
                                    s.Draw(noise, new Vector2(x, y), new Microsoft.Xna.Framework.Color(255, 255, 255, 4));
                                }
                            }
                            s.End();

                            s.Begin(blendState: BlendState.AlphaBlend);
                            s.Draw(textures["vignette"], g.Viewport.Bounds, new Microsoft.Xna.Framework.Color(1f, 1f, 1f, 1.0f));
                            s.End();
                        }
                        DrawCameraEffects();
                    }
                }

                g.SetRenderTarget(null);

                s.Begin();
                foreach (var (target, area, eye) in views)
                {
                    s.Draw(target, area, Microsoft.Xna.Framework.Color.White);
                }
                s.End();
            };
        }
    }
}