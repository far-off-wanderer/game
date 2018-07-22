namespace Far_Off_Wanderer
{
    using Far_Off_Wanderer.Scenes;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
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
                ContentManager = Content,
                GraphicsDevice = manager.GraphicsDevice
            };
            input = new Input();
            handlers.Run(All.Load(), content);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            song = Content.Load<Song>("ambient");
        }

        protected override void Update(GameTime gameTime)
        {
            if(MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Volume = .1f;
                SoundEffect.MasterVolume = .1f;
                MediaPlayer.Play(song);
            }

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