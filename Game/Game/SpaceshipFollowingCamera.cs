namespace Far_Off_Wanderer.Game;

using Microsoft.Xna.Framework;
using System;

public class SpaceshipFollowingCamera : Camera
{
    private Vector3 _target = Vector3.Zero;
    private Vector3 _position = Vector3.Zero;

    public Spaceship Ship { get; set; }
    public bool ZoomIn { get; set; }

    public override Vector3 Target
    {
        get
        {
            if (Ship != null)
            {
                _target = Ship.Position;
                if (ZoomIn == false)
                {
                    _target += Vector3.Transform(Vector3.Forward * 250f, Matrix.CreateRotationY(Ship.Yaw));
                }
            }
            return _target;
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public override Vector3 Position
    {
        get
        {
            if (Ship != null)
            {
                var bla = Ship.Speed;
                _position = Ship.Position
                    + (ZoomIn ? 0.3f : 1f) * (Ship.Forward * -100 + Ship.Up * 60 + Ship.Right * 0) * (float)Math.Sqrt(1 + 75 + Math.Abs(Ship.Speed / 2));
            }
            return _position;
        }
        set
        {
            throw new NotImplementedException();
        }
    }
}
