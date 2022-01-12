using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    class MenuHandler : Scenes.Handler<Scenes.Menu>
    {
        class InputActions
        {
            readonly Input input;

            public InputActions(Input input) => this.input = input;

            public bool CanceledFromScreen => input.TouchKeys.OnBackButton || input.Keyboard.On[(int)Keys.Escape];
            public bool ContinuesToNextScreen => input.Keyboard.On[(int)Keys.Space] || input.GamePad.On[Buttons.A] || input.TouchPanel.OnTouching;
        }

        public MenuHandler(Scenes.Menu scene)
        {
            var textures = new Dictionary<string, Texture2D>();

            Begin = async content =>
            {
                var bg = scene.Background;
                textures[bg.Portrait.Name] = content.Get<Texture2D>(bg.Portrait.Name);
                textures[bg.Landscape.Name] = content.Get<Texture2D>(bg.Landscape.Name);
            };

            Update = (gameTime, input) =>
            {
                var actions = new InputActions(input);

                if(actions.CanceledFromScreen)
                {
                    OnNext(scene.On.Cancel);
                }
                if(actions.ContinuesToNextScreen)
                {
                    OnNext(scene.On.Next);
                }
            };

            Draw = (graphics, started) =>
            {
                graphics.GraphicsDevice.Clear(Color.Black);
                graphics.SpriteBatch.Begin();
                var screen = graphics.GraphicsDevice.Viewport;
                var screenAspect = (float)screen.Width / screen.Height;
                var output = new Rectangle();
                var image = screenAspect > 1 ? scene.Background.Landscape : scene.Background.Portrait;
                var texture = textures[image.Name];
                var (Width, Height) = (image.Width ?? texture.Width, image.Height ?? texture.Height);
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
