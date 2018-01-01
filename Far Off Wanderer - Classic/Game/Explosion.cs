using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    public class Explosion : Object3D
    {
        public Explosion(string Id)
        {
            this.Id = Id;
        }
        public float EndOfLife { get; set; }
        public float MaxSize { get; set; }
        public float MinSize { get; set; }
        public float Age { get; set; }
        public float Spin { get; set; }
        public float StartSpin { get; set; }
        public float CurrentSize
        {
            get
            {
                return MathHelper.Lerp(MinSize, MaxSize, Age / EndOfLife);
            }
        }

        public override IEnumerable<Object3D> Update(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            Age += (float)ElapsedTime.TotalSeconds;
            if (Age > EndOfLife)
            {
                Alive = false;
            }
            yield break;
        }
    }
}
