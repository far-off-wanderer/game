namespace Far_Off_Wanderer.Game;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

public class Bullet : Object3D
{
    Vector3 direction;
    readonly float speed;
    double age;

    public Bullet(Vector3 Position, Vector3 Direction, float Speed)
    {
        this.Position = Position;

        Id = Data.Sparkle;
        Radius = 100;
        speed = Speed;
        age = 0;

        direction = Direction;
    }

    public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
    {
        Position += direction * (float)ElapsedTime.TotalSeconds * speed;
        age += ElapsedTime.TotalSeconds;
        if (age > 5)
        {
            Alive = false;
        }
        yield break;
    }

    public override IEnumerable<Explosion> Die(Environment Environment, Vector3 CollisionPoint)
    {
        if (Alive == true)
        {
            var dst = (Position - Environment.ActiveCamera.Position).Length();
            //Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
            //Environment.TriggerVibration(1 / (1 + dst));
        }
        Alive = false;

        yield return new Explosion(
            id: Data.Sparkle,
            position: Position,
            endOfLife: 5,
            minSize: 10,
            maxSize: 50,
            startSpin: 0,
            spin: Environment.Random.Real.Signed
        );
        for (int i = 0; i < 5; i++)
        {
            var position = new Vector3();
            do
            {
                position.X = Environment.Random.Real.Signed;
                position.Y = Environment.Random.Real.Signed;
                position.Z = Environment.Random.Real.Signed;
            } while (position.LengthSquared() > 1);
            var distance = position.Length();
            position *= Radius / 2;
            position += Position;

            yield return new Explosion(
                id: Data.Sparkle,
                position: position,
                endOfLife: (1 - distance) * 10 + 2.5f,
                minSize: (1 - distance) * 15 + 62.5f,
                maxSize: (1 - distance) * 75 + 250,
                startSpin: 0,
                spin: Environment.Random.Real.Signed
            );
        }
    }
}
