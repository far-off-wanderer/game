using Microsoft.Xna.Framework.Input.Touch;
using XNA = Microsoft.Xna.Framework.Input.Touch;

namespace Far_Off_Wanderer
{
    public class TouchPanel
    {
        TouchCollection? last;

        public bool OnTouching { get; private set; }
        public bool WhileTouching { get; private set; }

        public void Update()
        {
            var next = XNA.TouchPanel.GetState();
            last = last ?? next;

            OnTouching = next.Count > 0 && last.Value.Count == 0;
            WhileTouching = next.Count > 0;

            last = next;
        }
    }
}
