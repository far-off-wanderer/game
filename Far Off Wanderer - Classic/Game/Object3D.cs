using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace Conesoft.Game
{
    public abstract class Object3D
    {
        public string Id { get; set; }
        public virtual Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public BoundingSphere Boundary { get; set; }

        public bool Alive { get; protected set; }

        public Object3D()
        {
            Alive = true;
        }

        public virtual IEnumerable<Object3D> Update(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            yield break;
        }

        public virtual IEnumerable<Explosion> Die(DefaultEnvironment Environment, Vector3 CollisionPoint)
        {
            Alive = false;
            yield break;
        }

        private static BoundingSphere emptyBoundary = new BoundingSphere();
        public static BoundingSphere EmptyBoundary
        {
            get
            {
                return emptyBoundary;
            }
        }
    }
}
