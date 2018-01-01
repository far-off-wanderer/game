using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Conesoft.Game
{
    using Far_Off_Wanderer___Classic;

    public class Bullet : Object3D
    {
        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public double Age { get; set; }

        public Bullet(Vector3 Position, Vector3 Direction, float Speed)
        {
            this.Id = Data.Bullet;
            this.Position = Position;
            this.Orientation = Quaternion.Identity;
            this.Boundary = new BoundingSphere(Vector3.Zero, 100);
            this.Speed = Speed;
            this.Age = 0;

            this.Direction = Direction;
        }

        public override IEnumerable<Object3D> Update(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            Position += Direction * (float)ElapsedTime.TotalSeconds * Speed;
            Age += ElapsedTime.TotalSeconds;
            if (Age > 5)
            {
                Alive = false;
            }
            yield break;
        }
    }
}
