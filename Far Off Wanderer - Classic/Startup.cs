namespace Far_Off_Wanderer
{
    using Far_Off_Wanderer.Scenes;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Media;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal class Startup : Game
    {
        GraphicsDeviceManager manager;

        SceneHandlers handlers;
        Graphics graphics;
        Content content;
        Input input;
        Song song;

        public Startup()
        {
            manager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = Debugger.IsAttached == false
            };
            
            handlers = new SceneHandlers(onExit: Exit);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics = new Graphics
            {
                GraphicsDevice = manager.GraphicsDevice,
                SpriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice)
            };
            content = new Content
            {
                ContentManager = Content
            };
            input = new Input();
            handlers.Run(All.Load(), content);
        }

        protected override async void LoadContent()
        {
            base.LoadContent();
            song = Content.Load<Song>("ambient");
            await Task.Delay(5000);
            MediaPlayer.Play(song);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            input.Update();
            handlers.Update(gameTime.ElapsedGameTime, input);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            handlers.Draw(graphics);
        }

        internal void GoBack() => input.TouchKeys.TriggerBackButton();
    }
}