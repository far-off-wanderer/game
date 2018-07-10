﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using XNA = Microsoft.Xna.Framework.Input;

namespace Far_Off_Wanderer
{
    public class LevelHandler : Scenes.Handler<Scenes.Level>
    {
        public class InputActions
        {
            Input input;
            public InputActions(Input input) => this.input = input;

            public bool CancelingLevel => input.Keyboard.On[(int)Keys.Escape] || input.TouchKeys.OnBackButton;
            public bool CheckingForExitClickAfterGameOver => input.Keyboard.On[(int)Keys.Space] || input.GamePad.On[Buttons.A];
            public bool Toggling3dDisplay => input.Keyboard.On[(int)Keys.F3];
            public bool TogglingOverUnder => input.Keyboard.On[(int)Keys.F4];
            public bool ZoomingIn => input.GamePad.While[Buttons.DPadUp];
            // TODO: Add joysticks to Input
            public float CameraYaw => XNA.GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X;
            public float CameraPitch => XNA.GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y;

            public bool TurningLeft => input.Keyboard.While[(int)Keys.A] || input.Keyboard.While[(int)Keys.Left];
            public bool TurningRight => input.Keyboard.While[(int)Keys.D] || input.Keyboard.While[(int)Keys.Right];
            public bool Accelerating => input.Keyboard.While[(int)Keys.W] || input.Keyboard.While[(int)Keys.Up];
            public bool Decelerating => input.Keyboard.While[(int)Keys.S] || input.Keyboard.While[(int)Keys.Down];
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
            public static State<T> Track<T>(T value) => new State<T>(value);
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
            var accelerometer = new Accelerometer();
            
            var models = default(Dictionary<string, Model>);
            var textures = default(Dictionary<string, Texture2D>);
            var spriteFonts = default(Dictionary<string, SpriteFont>);
            var images = default(Dictionary<string, Image<Rgba32>>);

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
                Dictionary<string, T> Get<T>(params string[] names) => names.SelectAsDictionary(name => content.Get<T>(name));

                // at start for loading screen
                spriteFonts = Get<SpriteFont>(Data.Font);

                // delayed till game starts
                await Task.Run(() =>
                {
                    BoundingSphere CreateMerged(IEnumerable<BoundingSphere> spheres) => spheres.Aggregate((a, b) => BoundingSphere.CreateMerged(a, b));

                    models = Get<Model>(Data.Ship, Data.Drone, Data.Spaceship);
                    environment.Sounds = Get<Microsoft.Xna.Framework.Audio.SoundEffect>("puiiw", "explosion");
                    textures = Get<Texture2D>("vignette", "Floor", "arrow", "dot", "LDR_LLL1_0", "heightmap", scene.Surface.Texture, Data.BlackBackground, Data.Bullet, Data.GameOverOverlay, Data.GameWonOverlay, Data.Grass, Data.Sparkle);
                    images = Get<Image<Rgba32>>("heightmap");

                    var landscapes = images.Select(image => new Landscape(image.Key, content.GraphicsDevice, image.Value)
                    {
                        Radius = 40000,
                        Position = Vector3.UnitX * -10000 + Vector3.UnitY * -2500 + Vector3.UnitZ * -10000
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


                    environment.Acceleration = accelerometer.Acceleration;
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
                environment.ScreenSize = new Size(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

                leftEye = leftEye ?? new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.GraphicsDevice.DisplayMode.Width,
                    graphics.GraphicsDevice.DisplayMode.Height,
                    false,
                    graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );
                rightEye = rightEye ?? new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.GraphicsDevice.DisplayMode.Width,
                    graphics.GraphicsDevice.DisplayMode.Height,
                    false,
                    graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );

                var views = runIn3d ? new[]
                {
                    (
                        target: leftEye,
                        area: overUnder ? new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height / 2) : new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height),
                        eye: -30f
                    ),
                    (
                        target: rightEye,
                        area: overUnder ? new Rectangle(0, graphics.GraphicsDevice.Viewport.Height - graphics.GraphicsDevice.Viewport.Height / 2, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height / 2) : new Rectangle(graphics.GraphicsDevice.Viewport.Width - graphics.GraphicsDevice.Viewport.Width / 2, 0, graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height),
                        eye: 30f
                    )
                } : new[]
                {
                    (
                        target: leftEye,
                        area: new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height),
                        eye: 0f
                    )
                };
                foreach (var (target, area, eye) in views)
                {
                    graphics.GraphicsDevice.SetRenderTarget(target);

                    if(!started)
                    {
                        graphics.GraphicsDevice.Clear(Color.Black);
                        var text = "loading...";
                        var textSize = spriteFonts[Data.Font].MeasureString(text);
                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                        graphics.SpriteBatch.DrawString(spriteFonts[Data.Font], "loading...", (new Vector2(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height) - textSize) / 2, Color.White);
                        graphics.SpriteBatch.End();
                    }

                    if (started)
                    {

                        var camera = new CameraModel(level.Camera, eye, new Size(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height));

                        var basicEffect = new BasicEffect(graphics.GraphicsDevice);

                        var backgroundColor = scene.Environment.BackgroundColor.ToColor();

                        graphics.GraphicsDevice.Clear(backgroundColor);

                        graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                        graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                        basicEffect.EnableDefaultLighting();

                        basicEffect.LightingEnabled = true;
                        basicEffect.TextureEnabled = true;
                        basicEffect.VertexColorEnabled = false;
                        basicEffect.FogEnabled = true;
                        basicEffect.FogColor = backgroundColor.ToVector3();
                        basicEffect.FogStart = MathHelper.Lerp(20000f, 10f, scene.Environment.Fog);
                        basicEffect.FogEnd = 75000f;

                        basicEffect.DirectionalLight0.Enabled = true;
                        basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0, 0, 0);
                        basicEffect.DirectionalLight0.Direction = new Vector3(1, 0, 0);
                        basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0, 0);

                        basicEffect.PreferPerPixelLighting = true;
                        graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                        basicEffect.World = Matrix.Identity;
                        basicEffect.View = camera.View;
                        basicEffect.Projection = camera.Projection;

                        graphics.GraphicsDevice.BlendState = BlendState.Opaque;

                        foreach (var spaceship in level.Objects3D.OfType<Spaceship>())
                        {
                            var model = models[spaceship.Id];

                            model.Draw(Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateFromQuaternion(spaceship.Orientation * spaceship.ShipLeaning * spaceship.Strafing) * Matrix.CreateTranslation(spaceship.Position), camera.View, camera.Projection);
                        }

                        graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                        foreach (var landscape in level.Objects3D.OfType<Landscape>())
                        {
                            var color = scene.Surface.Color.ToColor();
                            var texture = scene.Surface.Texture != null ? textures[scene.Surface.Texture] : null;
                            landscape.Draw(basicEffect, color, texture);
                        }

                        var bounds = graphics.GraphicsDevice.Viewport.Bounds;

                        //graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                        //foreach (var landscape in level.Objects3D.OfType<Landscape>())
                        //{
                        //    var points = landscape.Points;
                        //    for (var y = 0; y < points.GetLength(1); y++)
                        //    {
                        //        for(var x = 0; x < points.GetLength(0); x++)
                        //        {
                        //            var point = landscape.Position + landscape.Radius * points[x, y];
                        //            var transformed = graphics.GraphicsDevice.Viewport.Project(point, camera.Projection, camera.View, Matrix.Identity);
                        //            var distance = (point - level.Camera.Position).Length();
                        //            if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                        //            {
                        //                var sprite = textures[Data.Sparkle];
                        //                var width = 1f * Math.Max(bounds.Width, bounds.Height) / distance;
                        //                width *= 40f;
                        //                var rectangle = new Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                        //                if (rectangle.Intersects(graphics.GraphicsDevice.Viewport.Bounds))
                        //                {
                        //                    graphics.SpriteBatch.Draw(sprite, rectangle, null, Color.Yellow, 0, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        //graphics.SpriteBatch.End();

                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                        foreach (var explosion in level.Objects3D.OfType<Explosion>())
                        {
                            var transformed = graphics.GraphicsDevice.Viewport.Project(explosion.Position, camera.Projection, camera.View, Matrix.Identity);
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
                                var rectangle = new Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                                if (rectangle.Intersects(graphics.GraphicsDevice.Viewport.Bounds))
                                {
                                    graphics.SpriteBatch.Draw(sprite, rectangle, null, new Color(2 - explosion.Age, 2 - explosion.Age, 1 - explosion.Age / 2, 2 - explosion.Age), explosion.StartSpin + explosion.Spin * explosion.Age, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                                }
                            }
                        }
                        graphics.SpriteBatch.End();
                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                        foreach (var bullet in level.Objects3D.OfType<Bullet>())
                        {
                            var transformed = graphics.GraphicsDevice.Viewport.Project(bullet.Position, camera.Projection, camera.View, Matrix.Identity);
                            var distance = (bullet.Position - level.Camera.Position).Length();
                            if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                            {
                                var sprite = textures[bullet.Id];
                                var width = Math.Max(bounds.Width, bounds.Height) * bullet.Radius / distance;
                                width *= 8f;
                                var rectangle = new Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                                if (rectangle.Intersects(graphics.GraphicsDevice.Viewport.Bounds))
                                {
                                    graphics.SpriteBatch.Draw(sprite, rectangle, null, Color.White, 0, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                                }
                            }
                        }
                        graphics.SpriteBatch.End();

                        var enemyCount = level.Objects3D.OfType<Spaceship>().Count();
                        var enemyCountText = (enemyCount > 0 ? enemyCount - 1 : 0).ToString();
                        var enemyCountSize = spriteFonts[Data.Font].MeasureString(enemyCountText).X;

                        var objectCount = level.Objects3D.Count();
                        var objectCountText = objectCount.ToString();
                        var objectCountSize = spriteFonts[Data.Font].MeasureString(objectCountText).X;

                        var osdBlend = Color.White * (1f - MathHelper.Clamp((fadeOut ?? 0) * 1.5f - 1, 0, 1));

                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                        graphics.SpriteBatch.DrawString(spriteFonts[Data.Font], enemyCountText, new Vector2(graphics.GraphicsDevice.Viewport.Width - enemyCountSize - 20, 10), osdBlend);

                        graphics.SpriteBatch.DrawString(spriteFonts[Data.Font], objectCountText, new Vector2(graphics.GraphicsDevice.Viewport.Width - objectCountSize - 20, 110), osdBlend);

                        if (fadeOut.HasValue)
                        {
                            graphics.SpriteBatch.Draw(textures[Data.BlackBackground], new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), new Color(0, 0, 0, fadeOut.Value / 2));

                            var splashTexture = textures[won.Value ? Data.GameWonOverlay : Data.GameOverOverlay];
                            var (Width, Height) = (1280, 768);

                            var screen = graphics.GraphicsDevice.Viewport;
                            var screenAspect = (float)screen.Width / screen.Height;
                            var output = new Rectangle();
                            var titleAspect = (float)Width / Height;
                            {
                                output.Width = Width * screen.Height / Height;
                                output.Height = screen.Height;
                                output.X = -(output.Width - screen.Width) / 2;
                                output.Y = 0;
                            }
                            if (dead.Value || won.Value)
                            {
                                graphics.SpriteBatch.Draw(splashTexture, output, new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                            }
                        }
                        graphics.SpriteBatch.End();

                        float Mod(float a, float b)
                        {
                            return a - b * (float)Math.Floor(a / b);
                        }

                        //if (scene.Surface.Heightmap != null)
                        {
                            graphics.SpriteBatch.Begin();
                            var floor = textures["heightmap"];
                            var floorSize = floor.Width * 2;
                            var maparea = new Rectangle((int)(graphics.GraphicsDevice.Viewport.Width - floorSize * 1.1f), (int)(graphics.GraphicsDevice.Viewport.Height - floorSize * 1.1f), floorSize, floorSize);
                            foreach (var ship in level.Objects3D.OfType<Spaceship>().OrderBy(s => s == level.LocalPlayer.ControlledObject))
                            {
                                var shipicon = ship.Id == Data.Ship ? textures["arrow"] : textures["dot"];
                                var shipsize = (1 / 8f) * (maparea.Width + maparea.Height) / 2f;
                                var shipcolor = level.LocalPlayer.ControlledObject == ship ? Color.White : Color.Red;
                                var shipposition = new Vector2(
                                    MathHelper.Lerp(maparea.Left, maparea.Right, Mod(ship.Position.X - environment.Range / 2f, environment.Range) / environment.Range),
                                    MathHelper.Lerp(maparea.Top, maparea.Bottom, Mod(ship.Position.Z - environment.Range / 2f, environment.Range) / environment.Range)
                                );
                                graphics.SpriteBatch.Draw(shipicon, shipposition, null, shipcolor, (float)Math.Atan2(ship.Forward.X, -ship.Forward.Z), Vector2.One * shipicon.Width / 2, shipsize / shipicon.Width, SpriteEffects.None, 0f);
                            }
                            graphics.SpriteBatch.End();

                            graphics.SpriteBatch.Begin();
                            graphics.SpriteBatch.Draw(floor, maparea, new Color(255, 255, 255, 4));
                            graphics.SpriteBatch.End();
                        }

                        graphics.SpriteBatch.Begin(blendState: BlendState.Additive);
                        var noise = textures["LDR_LLL1_0"];
                        for (var y = environment.Random.Next(1 - noise.Height, 0); y < graphics.GraphicsDevice.Viewport.Height; y += noise.Height)
                        {
                            for (var x = environment.Random.Next(1 - noise.Width, 0); x < graphics.GraphicsDevice.Viewport.Width; x += noise.Width)
                            {
                                graphics.SpriteBatch.Draw(noise, new Vector2(x, y), new Color(255, 255, 255, 4));
                            }
                        }
                        graphics.SpriteBatch.End();

                        graphics.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
                        graphics.SpriteBatch.Draw(textures["vignette"], graphics.GraphicsDevice.Viewport.Bounds, new Color(1f, 1f, 1f, 1.0f));
                        graphics.SpriteBatch.End();
                    }
                }

                graphics.GraphicsDevice.SetRenderTarget(null);

                graphics.SpriteBatch.Begin();
                foreach (var (target, area, eye) in views)
                {
                    graphics.SpriteBatch.Draw(target, area, Color.White);
                }
                graphics.SpriteBatch.End();
            };
        }
    }
}