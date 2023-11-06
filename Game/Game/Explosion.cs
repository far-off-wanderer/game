namespace Far_Off_Wanderer.Game;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Explosion : Object3D
{
    readonly float endOfLife;
    readonly float maxSize;
    readonly float minSize;
    float age;
    readonly float spin;
    readonly float startSpin;

    public float Age => age;
    public float Spin => spin;
    public float StartSpin => startSpin;

    public Explosion(string id, Vector3 position, float endOfLife, float minSize, float maxSize, float startSpin, float spin)
    {
        Id = id;

        Position = position;
        this.endOfLife = endOfLife;
        this.minSize = minSize;
        this.maxSize = maxSize;
        this.startSpin = startSpin;
        this.spin = spin;
    }

    public float CurrentSize => MathHelper.Lerp(minSize, maxSize, age / endOfLife);

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
