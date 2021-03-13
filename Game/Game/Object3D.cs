﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public abstract class Object3D
    {
        public string Id { get; set; }
        public virtual Vector3 Position { get; set; }
        public float HorizontalOrientation { get; set; }
        public float VerticalOrientation { get; set; }
        public float Radius { get; set; }

        public bool Alive { get; protected set; }

        public Object3D()
        {
            Alive = true;
        }

        public virtual IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            yield break;
        }

        public virtual IEnumerable<Explosion> Die(Environment Environment, Vector3 CollisionPoint)
        {
            Alive = false;
            yield break;
        }
    }
}
