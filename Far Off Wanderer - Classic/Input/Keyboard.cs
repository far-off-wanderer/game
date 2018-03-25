namespace Far_Off_Wanderer
{
    using Microsoft.Xna.Framework.Input;
    using System;
    using System.Linq;
    using XNA = Microsoft.Xna.Framework.Input;

    public class Keyboard
    {
        KeyboardState? last;
        public bool[] On { get; }
        public bool[] While { get; }

        public Keyboard()
        {
            var keyCount = Enum.GetValues(typeof(Keys)).Length;
            On = new bool[keyCount];
            While = new bool[keyCount];
        }

        public void Update()
        {
            var next = XNA.Keyboard.GetState();
            last = last ?? next;

            var nextKeys = next.GetPressedKeys();
            var lastKeys = last.Value.GetPressedKeys();

            for (var i = 0; i < On.Length; i++)
            {
                While[i] = nextKeys.Contains((Keys)i);
                On[i] = While[i] && lastKeys.Contains((Keys)i) == false;
            }

            last = next;
        }
    }
}
