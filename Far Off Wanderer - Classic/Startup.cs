namespace Far_Off_Wanderer
{
    using Far_Off_Wanderer.Scenes;
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    internal class Startup : Game
    {
        GraphicsDeviceManager manager;
        TheClassicGame theClassicGame;

        Handlers handlers;
        Graphics graphics;
        Content content;

        public Startup()
        {
            manager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = Debugger.IsAttached == false
            };

            theClassicGame = new TheClassicGame(manager, Content, Exit);
            
            handlers = new Handlers();

            var handlerTypes = typeof(Handler).GetTypeInfo().Assembly.DefinedTypes
                       .Where(t => t.IsSubclassOf(typeof(Handler)) && t.IsGenericType == false && t.BaseType.GetTypeInfo().IsGenericType);

            foreach(var handler in handlerTypes)
            {
                var generic = handler.BaseType.GetTypeInfo().GenericTypeArguments.First();
                var instance = Activator.CreateInstance(handler.AsType()) as Handler;
                handlers.Add(generic, instance);
            }

            handlers.Run(All.Load());

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
                handlers.Update(gameTime.ElapsedGameTime, content);
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