using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Far_Off_Wanderer
{
    class LevelHandler : Scenes.Handler<Scenes.Level>
    {
        public LevelHandler()
        {
            var startTime = default(TimeSpan?);
            var dead = false;
            var won = false;
            var started = false;
            var fadeOut = default(float?);
            var gameOverTouch = false;
            var gameOverKeyboard = false;
            var gameOverGamepad = false;
            var fadeIn = 0f;
            var fadeInTime = 0.25f;
            var playing = true;
            var sinceStart = default(TimeSpan);
            var environment = new Environment()
            {
                Random = new Random(),
                ModelBoundaries = new Dictionary<string, BoundingSphere>()
            };
            var level = default(Level);
            var accelerometer = new Accelerometer();
            var terrain = default(Terrain);


            var leftEye = default(RenderTarget2D);
            var rightEye = default(RenderTarget2D);

            void StartGame()
            {
                level = new Level(environment);

                startTime = null;
                dead = false;
                won = false;
                fadeOut = null;
                gameOverTouch = false;
                gameOverKeyboard = false;
                gameOverGamepad = false;

                fadeInTime = 0.25f;

                playing = true;
            }

            void CheckForExitClick()
            {
                if (TouchPanel.GetState().Count == 0)
                {
                    gameOverTouch = true;
                }
                if (TouchPanel.GetState().Count > 0 && gameOverTouch == true)
                {
                    OnGameOver();
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Space) == false)
                {
                    gameOverKeyboard = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Space) == true && gameOverKeyboard == true)
                {
                    OnGameOver();
                }

                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) == false)
                {
                    gameOverGamepad = true;
                }
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) == true && gameOverGamepad == true)
                {
                    OnGameOver();
                }
            }

            void OnGameOver()
            {
                playing = false;
            }

            void UpdateFadeout(GameTime e)
            {

                if (fadeOut.HasValue == false)
                {
                    fadeOut = 0;
                }
                else
                {
                    fadeOut += (float)e.ElapsedGameTime.TotalSeconds / 3;
                    if (fadeOut > 1)
                    {
                        fadeOut = 1;
                    }
                }
            }

            Begin = (scene, graphics) =>
            {
                var modelNames = new[] { Data.Ship, Data.Drone, Data.Spaceship };
                var models = modelNames.Select(name => new
                {
                    Name = name,
                    Model = graphics.GetModel(name)
                });
                var boundaries = models.Select(m => new
                {
                    m.Name,
                    Boundary = m.Model.Meshes.Select(mesh => mesh.BoundingSphere).Aggregate((a, b) => BoundingSphere.CreateMerged(a, b))
                });
                environment.ModelBoundaries = boundaries.ToDictionary(b => b.Name, b => b.Boundary);

                terrain = new Terrain(
                    position: Vector3.Down * 64 * 10,
                    size: new Vector3(1024 * 128, 256 * 40, 1024 * 128)
                );
                environment.Range = terrain.Size.X;
                terrain.LoadFromTexture2D(graphics.GetTexture(Data.LandscapeGround), environment);

                leftEye = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.GraphicsDevice.DisplayMode.Width,
                    graphics.GraphicsDevice.DisplayMode.Height,
                    false,
                    graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );
                rightEye = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.GraphicsDevice.DisplayMode.Width,
                    graphics.GraphicsDevice.DisplayMode.Height,
                    false,
                    graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PlatformContents
                );

                StartGame();
            };

            Update = (scene, gameTime) =>
            {
                environment.Update(gameTime.ElapsedGameTime);

                if (!playing)
                {
                    StartGame();
                }
                else
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        OnGameOver();
                    }
                    var e = new
                    {
                        TotalTime = gameTime.TotalGameTime,
                        ElapsedTime = gameTime.ElapsedGameTime
                    };

                    environment.Acceleration = accelerometer.Acceleration;
                    environment.ActiveCamera = level.Camera;
                    environment.Flipped = false; // Orientation == PageOrientation.LandscapeRight;



                    if (startTime.HasValue == false)
                    {
                        startTime = e.TotalTime;
                    }

                    sinceStart = e.TotalTime - startTime.Value;

                    fadeIn = Math.Min((float)sinceStart.TotalSeconds / fadeInTime, 1);

                    level.UpdateScene(
                        ElapsedTime: sinceStart.TotalSeconds > fadeInTime ? e.ElapsedTime : TimeSpan.Zero,
                        Yaw: GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X,
                        Pitch: GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y,
                        ZoomIn: GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)
                    );

                    if (started == false)
                    {
                        //resources.Sounds[Data.GoSound].Play();
                    }
                    started = true;

                    if (level.Objects3D.Contains(level.LocalPlayer.ControlledObject) == false && won != true)
                    {
                        if (dead != true)
                        {
                            //resources.Sounds[Data.GameOverSound].Play();
                        }
                        dead = true;
                        CheckForExitClick();
                        UpdateFadeout(gameTime);
                    }
                    else if (level.Objects3D.OfType<Spaceship>().Count() == 1 && dead == false)
                    {
                        if (won != true)
                        {
                            //resources.Sounds[Data.GoodSound].Play();
                            //UpdateHighscore();
                        }
                        won = true;
                        CheckForExitClick();
                        UpdateFadeout(gameTime);
                    }
                }
            };

            Draw = (scene, graphics) =>
            {
                environment.ScreenSize = new Size(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
                var views = new[] {
                (
                    target: leftEye,
//                    area: new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height),
                    area: new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height / 2),
                    eye: -30f
                ),
                (
                    target: rightEye,
//                    area: new Rectangle(graphics.GraphicsDevice.Viewport.Width - graphics.GraphicsDevice.Viewport.Width / 2, 0, graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height),
                    area: new Rectangle(0, graphics.GraphicsDevice.Viewport.Height - graphics.GraphicsDevice.Viewport.Height / 2, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height / 2),
                    eye: 30f
                )
            };
                views = new[]
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
                    if (playing)
                    {
                        var camera = new CameraModel(level.Camera, eye, new Size(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height));

                        var basicEffect = new BasicEffect(graphics.GraphicsDevice);

                        graphics.GraphicsDevice.Clear(level.Skybox.Color);

                        graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                        graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                        basicEffect.EnableDefaultLighting();

                        basicEffect.LightingEnabled = true;
                        basicEffect.TextureEnabled = true;
                        basicEffect.VertexColorEnabled = false;
                        basicEffect.FogEnabled = true;
                        basicEffect.FogColor = level.Skybox.Color.ToVector3();
                        basicEffect.FogStart = 10f;
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
                            var model = graphics.GetModel(spaceship.Id);

                            model.Draw(Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateFromQuaternion(spaceship.Orientation * spaceship.ShipLeaning * spaceship.Strafing) * Matrix.CreateTranslation(spaceship.Position), camera.View, camera.Projection);
                        }

                        graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                        var terrainPosition = terrain.Position;

                        var position = level.Camera.Position;
                        while (terrainPosition.X > position.X + terrain.Size.X / 2)
                        {
                            terrainPosition -= new Vector3(terrain.Size.X, 0, 0);
                        }
                        while (terrainPosition.Z > position.Z + terrain.Size.Z / 2)
                        {
                            terrainPosition -= new Vector3(0, 0, terrain.Size.Z);
                        }
                        while (terrainPosition.X < position.X - terrain.Size.X / 2)
                        {
                            terrainPosition += new Vector3(terrain.Size.X, 0, 0);
                        }
                        while (terrainPosition.Z < position.Z - terrain.Size.Z / 2)
                        {
                            terrainPosition += new Vector3(0, 0, terrain.Size.Z);
                        }

                        var range = 1;
                        for (var z = -range; z <= range; z++)
                        {
                            for (var x = -range; x <= range; x++)
                            {
                                terrain.Position = terrainPosition + new Vector3(x * terrain.Size.X, 0, z * terrain.Size.Z);
                                terrain.Draw(basicEffect, graphics.GetTexture(Data.Grass));
                            }
                        }
                        terrain.Position = terrainPosition;

                        var bounds = graphics.GraphicsDevice.Viewport.Bounds;

                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                        foreach (var explosion in level.Objects3D.OfType<Explosion>())
                        {
                            var transformed = graphics.GraphicsDevice.Viewport.Project(explosion.Position, camera.Projection, camera.View, Matrix.Identity);
                            var distance = (explosion.Position - level.Camera.Position).Length();
                            if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                            {
                                var sprite = graphics.GetTexture(Data.Sparkle);
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
                                var sprite = graphics.GetTexture(bullet.Id);
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
                        var enemyCountSize = graphics.GetSpriteFont(Data.Font).MeasureString(enemyCountText).X;

                        var objectCount = level.Objects3D.Count();
                        var objectCountText = objectCount.ToString();
                        var objectCountSize = graphics.GetSpriteFont(Data.Font).MeasureString(objectCountText).X;

                        var osdBlend = Color.White * (1f - MathHelper.Clamp((fadeOut ?? 0) * 1.5f - 1, 0, 1));

                        graphics.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                        graphics.SpriteBatch.DrawString(graphics.GetSpriteFont(Data.Font), enemyCountText, new Vector2(graphics.GraphicsDevice.Viewport.Width - enemyCountSize - 20, 10), osdBlend);

                        graphics.SpriteBatch.DrawString(graphics.GetSpriteFont(Data.Font), objectCountText, new Vector2(graphics.GraphicsDevice.Viewport.Width - objectCountSize - 20, 110), osdBlend);

                        if (fadeOut.HasValue)
                        {
                            graphics.SpriteBatch.Draw(graphics.GetTexture(Data.BlackBackground), new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), new Color(0, 0, 0, fadeOut.Value / 2));

                            var splashTexture = graphics.GetTexture(won ? Data.GameWonOverlay : Data.GameOverOverlay);
                            var (Width, Height) = (1280, 768);

                            var screen = graphics.GraphicsDevice.Viewport;
                            var screenAspect = (float)screen.Width / screen.Height;
                            var output = new Rectangle();
                            var titleAspect = (float)Width / Height;
                            //if (titleAspect > screenAspect)
                            {
                                output.Width = Width * screen.Height / Height;
                                output.Height = screen.Height;
                                output.X = -(output.Width - screen.Width) / 2;
                                output.Y = 0;
                            }
                            //else
                            {
                                output.Width = screen.Width;
                                output.Height = Height * screen.Width / Width;
                                output.X = 0;
                                output.Y = -(output.Height - screen.Height) / 2;
                            }

                            if (dead == true || won == true)
                            {
                                graphics.SpriteBatch.Draw(splashTexture, output, new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                            }
                        }
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
