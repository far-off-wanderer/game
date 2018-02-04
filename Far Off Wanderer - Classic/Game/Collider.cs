using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public class Collider : Object3D
    {
        Vector3 position;
        public Collider(Vector3 position, float radius)
        {
            this.Boundary = new BoundingSphere(position, radius);
            this.position = position;
            this.Alive = true;
        }

        public override Vector3 Position
        {
            get => position;
            set
            {
                Boundary = new BoundingSphere(value, Boundary.Radius);
                position = value;
            }
        }

        public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            yield break;
        }

        public override IEnumerable<Explosion> Die(Environment Environment, Vector3 CollisionPoint)
        {
            yield break;
        }
    }
}