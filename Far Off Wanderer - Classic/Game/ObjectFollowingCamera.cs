using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    public class SpaceshipFollowingCamera : Camera
    {
        private Vector3 _target = Vector3.Zero;
        private Vector3 _position = Vector3.Zero;

        public Spaceship Ship { get; set; }

        public override Vector3 Target
        {
            get
            {
                _target = Ship != null ? Ship.Position + Vector3.Transform(Vector3.Forward * 250f, Ship.Orientation) : _target;
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
                _position = Ship != null ? Ship.Position + Vector3.Transform((Vector3.Backward * 100 + Vector3.Up * 60 + Vector3.Right  * 0) * ((float)Math.Sqrt(1 + Math.Abs(Ship.Speed))), Ship.Orientation * Ship.ShipLeaning) : _position;
                return _position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
