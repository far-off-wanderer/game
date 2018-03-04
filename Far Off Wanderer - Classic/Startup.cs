namespace Far_Off_Wanderer
{
    using Microsoft.Xna.Framework;
    using System.Diagnostics;

    internal class Startup : Game
    {
        GraphicsDeviceManager manager;
        TheClassicGame theClassicGame;

        Scenes.All all;
        Scenes.Scene current;

        public Startup()
        {
            manager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = Debugger.IsAttached == false
            };

            theClassicGame = new TheClassicGame(manager, Content, Exit);

            all = Scenes.All.Load();
            current = all.Index;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            theClassicGame.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            theClassicGame.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            theClassicGame.Draw(gameTime);
        }

        internal void GoBack()
        {
            theClassicGame.GoBack();
        }
    }
}