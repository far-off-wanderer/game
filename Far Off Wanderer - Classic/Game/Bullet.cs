using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Conesoft.Game
{
    using Far_Off_Wanderer___Classic;

    public class Bullet : Object3D
    {
        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public double Age { get; set; }

        public Bullet(Vector3 Position, Vector3 Direction, float Speed)
        {
            this.Id = Data.Bullet;
            this.Position = Position;
            this.Orientation = Quaternion.Identity;
            this.Boundary = new BoundingSphere(Vector3.Zero, 100);
            this.Speed = Speed;
            this.Age = 0;

            this.Direction = Direction;
        }

        public override IEnumerable<Object3D> Update(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            Position += Direction * (float)ElapsedTime.TotalSeconds * Speed;
            Age += ElapsedTime.TotalSeconds;
            if (Age > 5)
            {
                Alive = false;
            }
            yield break;
        }

        public override IEnumerable<Explosion> Die(DefaultEnvironment Environment, Vector3 CollisionPoint)
        {
            if(Alive == true)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
                Environment.TriggerVibration(1 / (1 + dst / 5000));
            }
            Alive = false;

            yield return new Explosion(Data.Fireball)
            {
                Position = Position,
                EndOfLife = 5,
                MinSize = 50,
                MaxSize = 500,
                Spin = (float)Environment.Random.NextDouble() * 2 - 1
            };
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
                position *= Boundary.Radius * 2;
                position += Position;

                yield return new Explosion(Data.Fireball)
                {
                    EndOfLife = (1 - distance) * 10 + 2.5f,
                    MaxSize = (1 - distance) * 750 + 250,
                    MinSize = (1 - distance) * 150 + 62.5f,
                    Position = position,
                    Spin = (float)Environment.Random.NextDouble() * 2 - 1
                };
            }
        }
    }
}
