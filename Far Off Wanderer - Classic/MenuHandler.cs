using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Far_Off_Wanderer
{
    class MenuHandler : Scenes.Handler<Scenes.Menu>
    {
        public MenuHandler()
        {
            var startPlayingKeyup = false;
            var startPlayingButtonup = false;
            var startPlayingTouchup = false;

            Update = (scene, gameTime) =>
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
                    OnNext(scene.NextScene);
                }
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) == false)
                {
                    startPlayingButtonup = true;
                }
                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) && startPlayingButtonup == true)
                {
                    startPlayingButtonup = false;
                    OnNext(scene.NextScene);
                }
                if (TouchPanel.GetState().Count == 0)
                {
                    startPlayingTouchup = true;
                }
                if (TouchPanel.GetState().Count > 0 && startPlayingTouchup == true)
                {
                    startPlayingTouchup = false;
                    OnNext(scene.NextScene);
                }
            };

            Draw = (scene, graphics) =>
            {
                graphics.GraphicsDevice.Clear(Color.Black);
                graphics.SpriteBatch.Begin();
                var screen = graphics.GraphicsDevice.Viewport;
                var screenAspect = (float)screen.Width / screen.Height;
                var output = new Rectangle();
                var image = screenAspect > 1 ? scene.Background.Landscape : scene.Background.Portrait;
                var texture = graphics.GetTexture(image.Name);
                var (Width, Height) = (image.Width, image.Height);
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
                graphics.SpriteBatch.Draw(texture, output, Color.White);
                graphics.SpriteBatch.End();
            };
        }
    }
}
