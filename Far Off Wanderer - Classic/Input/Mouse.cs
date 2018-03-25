namespace Portable.Input
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using System.Linq;

    public class Mouse
    {
        MouseState last;
        MouseState current;

        //public ButtonStates On { get; } = new ButtonStates();
        //public ButtonStates While { get; } = new ButtonStates();

        //public float WheelPosition => current.WheelPosition;
        //public Vector2 Movement => current.Movement;

        //public float WheelMovement => current.WheelPosition - last.WheelPosition;

        //public void Clear() => last = null;

        //public void Update(MouseState next)
        //{
        //    last = current ?? next;
        //    On.Update(next.Buttons.Except(last.Buttons).ToArray(), next.Movement);
        //    While.Update(next.Buttons.Union(last.Buttons).ToArray(), next.Movement);
        //    current = next;
        //}
    }
}
