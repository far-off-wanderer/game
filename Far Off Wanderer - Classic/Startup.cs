namespace Far_Off_Wanderer
{
    using Far_Off_Wanderer.Scenes;
    using Microsoft.Xna.Framework;
    using System.Diagnostics;

    internal class Startup : Game
    {
        GraphicsDeviceManager manager;
        TheClassicGame theClassicGame;

        Handlers handlers;
        Graphics graphics;

        public Startup()
        {
            manager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = Debugger.IsAttached == false
            };

            theClassicGame = new TheClassicGame(manager, Content, Exit);
            
            handlers = new Handlers();

            handlers.Add(new MenuHandler());

            handlers.Run(All.Load());

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics = new Graphics
            {
                GraphicsDevice = manager.GraphicsDevice,
                ContentManager = Content,
                SpriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice)
            };
            theClassicGame.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(handlers.IsActive)
            {
                handlers.Update(gameTime);
            }
            else
            {
                theClassicGame.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if(handlers.IsActive)
            {
                handlers.Draw(graphics);
            }
            else
            {
                theClassicGame.Draw(gameTime);
            }
        }

        internal void GoBack()
        {
            theClassicGame.GoBack();
        }
    }
}