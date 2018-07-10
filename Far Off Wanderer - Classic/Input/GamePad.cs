using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using XNA = Microsoft.Xna.Framework.Input;

namespace Far_Off_Wanderer
{
    public class GamePad
    {
        GamePadState? last;
        public Dictionary<Buttons, bool> On { get; private set; } = new Dictionary<Buttons, bool>();
        public Dictionary<Buttons, bool> While { get; private set; } = new Dictionary<Buttons, bool>();

        readonly Buttons[] buttons;

        public GamePad()
        {
            this.buttons = Enum.GetValues(typeof(Buttons)).Cast<Buttons>().ToArray();
        }

        public void Update()
        {
            var next = XNA.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
            last = last ?? next;

            foreach(var button in buttons)
            {
                On[button] = next.IsButtonDown(button) && last.Value.IsButtonDown(button) == false;
                While[button] = next.IsButtonDown(button);
            }

            last = next;
        }
    }
}
