using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    class Orbit : Object3D
    {
        Spaceship orbitee;
        Object3D orbiter;
        float angle;
        readonly float angularSpeed;
        Vector3 toOrbiter;

        public Orbit(Spaceship orbitee, Object3D orbiter, float speed)
        {
            this.orbitee = orbitee;
            this.orbiter = orbiter;
            this.angularSpeed = speed;
            this.toOrbiter = orbiter.Position - orbitee.Position;
        }

        public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            if(orbiter.Alive == false)
            {
                Alive = false;
                yield break;
            }

            angle += angularSpeed * 2f * (float)Math.PI * (float)ElapsedTime.TotalSeconds;
            if(angle > 2f * Math.PI)
            {
                angle -= 2f * (float)Math.PI;
            }
            if(angle < 0)
            {
                angle += 2f * (float)Math.PI;
            }

            orbiter.Position = orbitee.Position + Vector3.Transform(toOrbiter, Quaternion.CreateFromAxisAngle(orbitee.Forward, angle));

            yield break;
        }
    }
}
