using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public class Bullet : Object3D
    {
        Vector3 direction;
        float speed;
        double age;

        public Bullet(Vector3 Position, Vector3 Direction, float Speed)
        {
            this.Position = Position;

            Id = Data.Bullet;
            Orientation = Quaternion.Identity;
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
            if(Alive == true)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
                Environment.TriggerVibration(1 / (1 + dst));
            }
            Alive = false;

            yield return new Explosion(
                id: Data.Fireball,
                position: Position,
                endOfLife: 5,
                minSize: 50,
                maxSize: 500,
                startSpin: 0,
                spin: (float)Environment.Random.NextDouble() * 2 - 1
            );
            for (int i = 0; i < 20; i++)
            {
                var position = new Vector3();
                do
                {
                    position.X = (float)Environment.Random.NextDouble() * 2f - 1f;
                    position.Y = (float)Environment.Random.NextDouble() * 2f - 1f;
                    position.Z = (float)Environment.Random.NextDouble() * 2f - 1f;
                } while (position.LengthSquared() > 1);
                var distance = position.Length();
                position *= Radius * 2;
                position += Position;

                yield return new Explosion(
                    id: Data.Fireball,
                    position: position,
                    endOfLife: (1 - distance) * 10 + 2.5f,
                    minSize: (1 - distance) * 150 + 62.5f,
                    maxSize: (1 - distance) * 750 + 250,
                    startSpin: 0,
                    spin: (float)Environment.Random.NextDouble() * 2 - 1
                );
            }
        }
    }
}
