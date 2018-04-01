using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public bool Shooting => input.Keyboard.While[(int)Keys.Space];
            public bool StrafingLeft => input.Keyboard.On[(int)Keys.Q];
            public bool StrafingRight => input.Keyboard.On[(int)Keys.E];
        }

        public LevelHandler(Scenes.Level scene)
        {
            var dead = false;
            var won = false;
            var started = false;
            var fadeOut = default(float?);
            var fadeIn = 0f;
            var fadeInTime = 0.25f;
            var runtime = default(TimeSpan);
            var environment = new Environment()
            {
                Random = new Random(),
                ModelBoundaries = new Dictionary<string, BoundingSphere>()
            };
            var level = default(Level);
            var accelerometer = new Accelerometer();
            var terrain = default(Terrain);

            var models = new Dictionary<string, Model>();
            var textures = new Dictionary<string, Texture2D>();
            var spriteFonts = new Dictionary<string, SpriteFont>();

            var runIn3d = false;
            var overUnder = true;
            var leftEye = default(RenderTarget2D);
            var rightEye = default(RenderTarget2D);

            void StartGame()
            {
                level = new Level(environment);

                runtime = TimeSpan.Zero;
                dead = false;
                won = false;
                fadeOut = null;

                fadeInTime = 0.25f;
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

            Begin = content =>
            {
                var modelNames = new[] { Data.Ship, Data.Drone, Data.Spaceship };
                var loadedModels = modelNames.Select(name => new
                {
                    Name = name,
                    Model = content.GetModel(name)
                });
                var boundaries = loadedModels.Select(m => new
                {
                    m.Name,
                    Boundary = m.Model.Meshes.Select(mesh => mesh.BoundingSphere).Aggregate((a, b) => BoundingSphere.CreateMerged(a, b))
                });
                environment.ModelBoundaries = boundaries.ToDictionary(b => b.Name, b => b.Boundary);

                environment.Sounds = new Dictionary<string, Microsoft.Xna.Framework.Audio.SoundEffect>
                {
                    ["puiiw"] = content.GetSoundEffect("puiiw")
                };

                foreach (var name in modelNames)
                {
                    models[name] = content.GetModel(name);
                }

                var textureNames = new[] { "vignette", "Floor", "arrow", "dot", "LDR_LLL1_0", scene.Surface.Texture, Data.BlackBackground, Data.Bullet, Data.GameOverOverlay, Data.GameWonOverlay, Data.Grass, Data.Sparkle };
                textureNames = textureNames.Distinct().Where(n => n != null).ToArray();

                foreach (var name in textureNames)
                {
                    textures[name] = content.GetTexture(name);
                }

                var spriteFontNames = new[] { Data.Font };

                foreach (var name in spriteFontNames)
                {
                    spriteFonts[name] = content.GetSpriteFont(name);
                }

                terrain = new Terrain(
                    position: Vector3.Down * 64 * 10,
                    size: new Vector3(1024 * 128, 256 * 40, 1024 * 128)
                );
                environment.Range = terrain.Size.X;
                terrain.LoadFromTexture2D(content.GetTexture(Data.LandscapeGround), environment);

                StartGame();
            };

            Update = (timeSpan, input) =>
            {
                var actions = new InputActions(input);

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
                    OnNext(won ? scene.On.Won : scene.On.GameOver);
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
                }
                else if (level.Objects3D.OfType<Spaceship>().Count() == 1 && dead == false)
                {
                    if (won != true)
                    {
                        //resources.Sounds[Data.GoodSound].Play();
                        //UpdateHighscore();
                    }
                    won = true;
                }
                if (input.Keyboard.On[(int)Keys.O])
                {
                    won = true;
                }
                if (won == true || dead == true)
                {
                    if (actions.CheckingForExitClickAfterGameOver)
                    {
                        OnGameOver();
                    };
                    UpdateFadeout(timeSpan);
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
                            var color = scene.Surface.Color.ToColor();
                            var texture = scene.Surface.Texture != null ? textures[scene.Surface.Texture] : null;
                            terrain.Draw(basicEffect, color, texture);
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

                        var splashTexture = textures[won ? Data.GameWonOverlay : Data.GameOverOverlay];
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
                        if (dead || won)
                        {
                            graphics.SpriteBatch.Draw(splashTexture, output, new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                        }
                    }
                    graphics.SpriteBatch.End();

                    float Mod(float a, float b)
                    {
                        return a - b * (float)Math.Floor(a / b);
                    }

                    graphics.SpriteBatch.Begin();
                    var floor = textures["Floor"];
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