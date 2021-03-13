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
            this.Radius = radius;
            this.position = position;
            this.Alive = true;
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