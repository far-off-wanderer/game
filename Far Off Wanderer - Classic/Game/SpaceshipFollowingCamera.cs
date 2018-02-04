using Microsoft.Xna.Framework;
using System;

namespace Conesoft.Game
{
    public class SpaceshipFollowingCamera : Camera
    {
        private Vector3 _target = Vector3.Zero;
        private Vector3 _position = Vector3.Zero;

        public Spaceship Ship { get; set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool ZoomIn { get; set; }

        public override Vector3 Target
        {
            get
            {
                if(Ship != null)
                {
                    _target = Ship.Position;
                    if (ZoomIn == false)
                    {
                        _target += Vector3.Transform(Vector3.Forward * 250f, Ship.Orientation);
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
                if(Ship != null)
                {
                    var bla = Ship.Speed;
                    _position = Ship.Position
                        + Vector3.Transform(
                            (ZoomIn ? 0.3f : 1f)
                            * (Vector3.Backward * 100 + Vector3.Up * 60 + Vector3.Right * 0)
                            * ((float)Math.Sqrt(1 + 75 + Math.Abs(Ship.Speed / 2)))
                            ,
                            Ship.Orientation
                            * Quaternion.CreateFromYawPitchRoll(Yaw * (float)Math.PI, Pitch, 0)
                            * Ship.ShipLeaning
                        );
                }
                return _position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
