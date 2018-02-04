using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Far_Off_Wanderer
{
    public class Explosion : Object3D
    {
        float endOfLife;
        float maxSize;
        float minSize;
        float age;
        float spin;
        float startSpin;

        public float Age => age;
        public float Spin => spin;
        public float StartSpin => startSpin;

        public Explosion(string id, Vector3 position, float endOfLife, float minSize, float maxSize, float startSpin, float spin)
        {
            this.Id = id;

            this.Position = position;
            this.endOfLife = endOfLife;
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.startSpin = startSpin;
            this.spin = spin;
        }

        public float CurrentSize
        {
            get
            {
                return MathHelper.Lerp(minSize, maxSize, age / endOfLife);
            }
        }

        public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            age += (float)ElapsedTime.TotalSeconds;
            if (age > endOfLife)
            {
                Alive = false;
            }
            yield break;
        }
    }
}
