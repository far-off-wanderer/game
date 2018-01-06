using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace Conesoft.Game
{
    public class ThrustFlame
    {
        private Queue<Flame> flames;
        private Vector3? LastPosition;
        private Vector3? LastDirection;
        public float ThrustBackshift { get; set; }

        public ThrustFlame()
        {
            flames = new Queue<Flame>();
        }

        public void UpdateThrust(Vector3 Position, Vector3 Direction, Vector3 Up, TimeSpan ElapsedTime, DefaultEnvironment Environment)
        {
            if (LastPosition.HasValue && LastDirection.HasValue)
            {
                var distance = Position - LastPosition.Value;
                if (distance.LengthSquared() < Math.Pow(Environment.Range / 2, 2))
                {
                    var count = (Position - LastPosition.Value).Length() / 40;
                    Position -= Vector3.Normalize(Direction) * ThrustBackshift;
                    if (count > 4)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var value = (float)(i + Environment.Random.NextDouble()) / (float)count;

                            var flame = new Flame()
                            {
                                Position = Vector3.Hermite(LastPosition.Value, LastDirection.Value, Position, Direction, value) + Environment.RandomPointInUnitSphere() * 0.1f,
                                Up = Up,
                                Direction = -Direction
                            };
                            flames.Enqueue(flame);
                        }
                    }
                }
            }
            LastDirection = Direction;
            LastPosition = Position;
        }

        public Queue<Flame> Flames
        {
            get
            {
                return flames;
            }
        }

        public void DontThrust()
        {
            LastDirection = null;
            LastPosition = null;
        }

        public struct Flame
        {
            public Vector3 Position;
            public Vector3 Direction; // points into the oposite direction of the spaceship, btw..
            public Vector3 Up;
        }
    }
}