namespace Far_Off_Wanderer;

using Far_Off_Wanderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

internal class Startup : Microsoft.Xna.Framework.Game
{
    readonly GraphicsDeviceManager manager;
    readonly SceneHandlers handlers;
    Graphics graphics;
    Content content;
    Input input;
    Song song;

    public Startup()
    {
        var shouldGoFullscreen = Debugger.IsAttached == false;

        manager = new GraphicsDeviceManager(this);
        manager.ApplyChanges();

        manager.IsFullScreen = shouldGoFullscreen;
        manager.PreferredBackBufferWidth = shouldGoFullscreen ? GraphicsDevice.Adapter.CurrentDisplayMode.Width : 1280;
        manager.PreferredBackBufferHeight = shouldGoFullscreen ? GraphicsDevice.Adapter.CurrentDisplayMode.Height : 720;
        manager.ApplyChanges();

        IsMouseVisible = manager.IsFullScreen == false;

        handlers = new SceneHandlers(onExit: Exit);

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();

        graphics = new Graphics(graphicsDevice: manager.GraphicsDevice, spriteBatch: new SpriteBatch(manager.GraphicsDevice));
        content = new Content(contentManager: Content, graphicsDevice: manager.GraphicsDevice);

        input = new Input();
        handlers.Run(All.Load(), content, LaunchParameters.ContainsKey("-scene") ? LaunchParameters["-scene"] : null);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        song = Content.Load<Song>("ambient");
    }

    protected override void Update(GameTime gameTime)
    {
        if (MediaPlayer.State != MediaState.Playing)
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
