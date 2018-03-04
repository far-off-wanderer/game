namespace Far_Off_Wanderer
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Input.Touch;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.Foundation;

    internal class TheClassicGame
    {
        IResources resources;
        IAccelerometer accelerometer;

        SpriteBatch spriteBatch;
        GraphicsDevice graphics;
        TimeSpan? startTime;

        float fadeInTime;
        float fadeIn;
        float? fadeOut;
        TimeSpan sinceStart;
        bool gameOverTouch;
        bool dead;
        bool won;
        bool started;
        Environment environment;

        Level level;
        private bool gameOverKeyboard;
        private bool gameOverGamepad;

        Texture2D titleScreenLandscape;
        Texture2D titleScreenPortrait;
        bool playing;
        bool startPlayingKeyup;
        bool startPlayingButtonup;
        bool startPlayingTouchup;

        RenderTarget2D leftEye;
        RenderTarget2D rightEye;

        GraphicsDeviceManager graphicsDeviceManager;
        ContentManager contentManager;
        Action exitCallback;
        private GraphicsDevice GraphicsDevice => graphicsDeviceManager.GraphicsDevice;
        private ContentManager Content => contentManager;
        private Action Exit => exitCallback;

        internal TheClassicGame(GraphicsDeviceManager graphicsDeviceManager, ContentManager contentManager, Action exitCallback)
        {
            this.graphicsDeviceManager = graphicsDeviceManager;
            this.contentManager = contentManager;
            this.exitCallback = exitCallback;

            resources = new Resources(new ResourceLoader(contentManager));
            accelerometer = new Accelerometer();
        }

        internal void StartGame()
        {
            level = new Level(environment);

            var terrain = resources.Terrains[Data.Landscape];

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

        internal void Initialize()
        {
            graphics = GraphicsDevice;

            graphics.PresentationParameters.PresentationInterval = PresentInterval.Immediate;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            titleScreenLandscape = Content.Load<Texture2D>("titlescreen-landscape");
            titleScreenPortrait = Content.Load<Texture2D>("titlescreen-portrait");

            environment = new Environment()
            {
                Random = new Random(),
                ModelBoundaries = new Dictionary<string, BoundingSphere>()
            };

            resources.Load((to, from) =>
            {
                to.Models.Add(from.Load<Model>(Data.Ship, Data.Drone, Data.Spaceship));
                to.Sprites.Add(from.Load<Texture2D>(Data.Fireball, Data.Energyball, Data.Bullet, Data.Sparkle, Data.Grass, /*Data.TutorialOverlay, */Data.GameWonOverlay, Data.GameOverOverlay));

                var texture = new Texture2D(graphics, 1, 1);
                texture.SetData(new Color[] { Color.Black });
                to.Sprites.Add(Data.BlackBackground, texture);

                to.Sounds.Add(from.Load<SoundEffect>(Data.ExplosionSound, Data.GameOverSound, Data.GoSound, Data.GoodSound, Data.LaserSound));

                to.Fonts.Add(from.Load<SpriteFont>(Data.Font, Data.SmallFont));

                var terrain = new Terrain(
                    position: Vector3.Down * 64 * 10,
                    size: new Vector3(1024 * 128, 256 * 40, 1024 * 128)
                );
                environment.Range = terrain.Size.X;
                terrain.LoadFromTexture2D(from.Load<Texture2D>(Data.LandscapeGround), environment);
                to.Terrains.Add(Data.Landscape, terrain);

                foreach (var model in to.Models)
                {
                    var boundingSphere = default(BoundingSphere);
                    foreach (var mesh in model.Value.Meshes)
                    {
                        boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
                    }
                    environment.ModelBoundaries[model.Key] = boundingSphere;
                }
            });

            environment.Sounds = resources.Sounds;

            leftEye = new RenderTarget2D(graphics,
                graphics.DisplayMode.Width,
                graphics.DisplayMode.Height,
                false,
                graphics.PresentationParameters.BackBufferFormat,
                graphics.PresentationParameters.DepthStencilFormat,
                graphics.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.PlatformContents
            );
            rightEye = new RenderTarget2D(graphics,
                graphics.DisplayMode.Width,
                graphics.DisplayMode.Height,
                false,
                graphics.PresentationParameters.BackBufferFormat,
                graphics.PresentationParameters.DepthStencilFormat,
                graphics.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.PlatformContents
            );
        }

        internal void Update(GameTime gameTime)
        {
            environment.Update(gameTime.ElapsedGameTime);

            if (!playing)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    //Exit();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Space) == false)
                {
                    startPlayingKeyup = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && startPlayingKeyup == true)
                {
                    startPlayingKeyup = false;
                    StartGame();
                }
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) == false)
                {
                    startPlayingButtonup = true;
                }
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) && startPlayingKeyup == true)
                {
                    startPlayingButtonup = false;
                    StartGame();
                }
                if (TouchPanel.GetState().Count == 0)
                {
                    startPlayingTouchup = true;
                }
                if (TouchPanel.GetState().Count > 0 && startPlayingTouchup == true)
                {
                    startPlayingTouchup = false;
                    StartGame();
                }
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
                environment.ScreenSize = new Size(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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
                    resources.Sounds[Data.GoSound].Play();
                }
                started = true;

                if (level.Objects3D.Contains(level.LocalPlayer.ControlledObject) == false && won != true)
                {
                    if (dead != true)
                    {
                        resources.Sounds[Data.GameOverSound].Play();
                    }
                    dead = true;
                    CheckForExitClick();
                    UpdateFadeout(gameTime);
                }
                else if (level.Objects3D.OfType<Spaceship>().Count() == 1 && dead == false)
                {
                    if (won != true)
                    {
                        resources.Sounds[Data.GoodSound].Play();
                        //UpdateHighscore();
                    }
                    won = true;
                    CheckForExitClick();
                    UpdateFadeout(gameTime);
                }
            }
        }

        private void CheckForExitClick()
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

        private void OnGameOver()
        {
            playing = false;
        }

        private void UpdateFadeout(GameTime e)
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

        internal void Draw(GameTime gameTime)
        {
            var views = new[] {
                (
                    target: leftEye,
//                    area: new Rectangle(0, 0, graphics.Viewport.Width / 2, graphics.Viewport.Height),
                    area: new Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height / 2),
                    eye: -30f
                ),
                (
                    target: rightEye,
//                    area: new Rectangle(graphics.Viewport.Width - graphics.Viewport.Width / 2, 0, graphics.Viewport.Width / 2, graphics.Viewport.Height),
                    area: new Rectangle(0, graphics.Viewport.Height - graphics.Viewport.Height / 2, graphics.Viewport.Width, graphics.Viewport.Height / 2),
                    eye: 30f
                )
            };
            views = new[]
            {
                (
                    target: leftEye,
                    area: new Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height),
                    eye: 0f
                )
            };
            foreach (var (target, area, eye) in views)
            {
                graphics.SetRenderTarget(target);
                if (!playing)
                {
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin();
                    var screen = GraphicsDevice.Viewport;
                    var screenAspect = (float)screen.Width / screen.Height;
                    var output = new Rectangle();
                    var titleTexture = screenAspect > 1 ? titleScreenLandscape : titleScreenPortrait;
                    var (Width, Height) = screenAspect > 1 ? (1836, 1200) : (1080, 1920);
                    var titleAspect = (float)Width / Height;
                    if (titleAspect > screenAspect)
                    {
                        output.Width = Width * screen.Height / Height;
                        output.Height = screen.Height;
                        output.X = -(output.Width - screen.Width) / 2;
                        output.Y = 0;
                    }
                    else
                    {
                        output.Width = screen.Width;
                        output.Height = Height * screen.Width / Width;
                        output.X = 0;
                        output.Y = -(output.Height - screen.Height) / 2;
                    }
                    spriteBatch.Draw(titleTexture, output, Color.White);
                    spriteBatch.End();
                }
                else
                {
                    var camera = new CameraModel(level.Camera, eye, new Size(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

                    var basicEffect = new BasicEffect(graphics);

                    graphics.Clear(level.Skybox.Color);

                    graphics.BlendState = BlendState.Opaque;
                    graphics.DepthStencilState = DepthStencilState.Default;

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
                    graphics.RasterizerState = RasterizerState.CullNone;

                    basicEffect.World = Matrix.Identity;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    graphics.BlendState = BlendState.Opaque;

                    foreach (var spaceship in level.Objects3D.OfType<Spaceship>())
                    {
                        var model = resources.Models[spaceship.Id];

                        model.Draw(Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateFromQuaternion(spaceship.Orientation * spaceship.ShipLeaning * spaceship.Strafing) * Matrix.CreateTranslation(spaceship.Position), camera.View, camera.Projection);
                    }

                    graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                    var terrain = resources.Terrains[Data.Landscape];
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
                            terrain.Draw(basicEffect, resources.Sprites[Data.Grass]);
                        }
                    }
                    terrain.Position = terrainPosition;

                    var bounds = graphics.Viewport.Bounds;

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                    foreach (var explosion in level.Objects3D.OfType<Explosion>())
                    {
                        var transformed = graphics.Viewport.Project(explosion.Position, camera.Projection, camera.View, Matrix.Identity);
                        var distance = (explosion.Position - level.Camera.Position).Length();
                        if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                        {
                            var sprite = resources.Sprites[Data.Sparkle];
                            var width = explosion.CurrentSize * Math.Max(bounds.Width, bounds.Height) / distance;
                            width *= 4f;
                            if(explosion.Id == Data.Fireball)
                            {
                                width *= 2f;
                            }
                            var rectangle = new Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                            if (rectangle.Intersects(graphics.Viewport.Bounds))
                            {
                                spriteBatch.Draw(sprite, rectangle, null, new Color(2 - explosion.Age, 2 - explosion.Age, 1 - explosion.Age / 2, 2 - explosion.Age), explosion.StartSpin + explosion.Spin * explosion.Age, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                            }
                        }
                    }
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                    foreach (var bullet in level.Objects3D.OfType<Bullet>())
                    {
                        var transformed = graphics.Viewport.Project(bullet.Position, camera.Projection, camera.View, Matrix.Identity);
                        var distance = (bullet.Position - level.Camera.Position).Length();
                        if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                        {
                            var sprite = resources.Sprites[bullet.Id];
                            var width = Math.Max(bounds.Width, bounds.Height) * bullet.Radius / distance;
                            width *= 8f;
                            var rectangle = new Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                            if (rectangle.Intersects(graphics.Viewport.Bounds))
                            {
                                spriteBatch.Draw(sprite, rectangle, null, Color.White, 0, new Vector2(sprite.Width / 2), SpriteEffects.None, transformed.Z);
                            }
                        }
                    }
                    spriteBatch.End();

                    var enemyCount = level.Objects3D.OfType<Spaceship>().Count();
                    var enemyCountText = (enemyCount > 0 ? enemyCount - 1 : 0).ToString();
                    var enemyCountSize = resources.Fonts[Data.Font].MeasureString(enemyCountText).X;

                    var objectCount = level.Objects3D.Count();
                    var objectCountText = objectCount.ToString();
                    var objectCountSize = resources.Fonts[Data.Font].MeasureString(objectCountText).X;

                    var osdBlend = Color.White * (1f - MathHelper.Clamp((fadeOut ?? 0) * 1.5f - 1, 0, 1));

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                    spriteBatch.DrawString(resources.Fonts[Data.Font], enemyCountText, new Vector2(graphics.Viewport.Width - enemyCountSize - 20, 10), osdBlend);

                    spriteBatch.DrawString(resources.Fonts[Data.Font], objectCountText, new Vector2(graphics.Viewport.Width - objectCountSize - 20, 110), osdBlend);

                    if (fadeOut.HasValue)
                    {
                        spriteBatch.Draw(resources.Sprites[Data.BlackBackground], new Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), new Color(0, 0, 0, fadeOut.Value / 2));

                        var splashTexture = resources.Sprites[won ? Data.GameWonOverlay : Data.GameOverOverlay];
                        var (Width, Height) = (1280, 768);

                        var screen = GraphicsDevice.Viewport;
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
                            spriteBatch.Draw(splashTexture, output, new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                        }
                    }
                    spriteBatch.End();
                }
            }

            graphics.SetRenderTarget(null);

            spriteBatch.Begin();
            foreach (var (target, area, eye) in views)
            {
                spriteBatch.Draw(target, area, Color.White);
            }
            spriteBatch.End();
        }

        internal void GoBack()
        {
            if (playing)
            {
                OnGameOver();
            }
            else
            {
                Exit();
            }
        }
    }
}