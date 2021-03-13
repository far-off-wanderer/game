using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public class ThrustFlame
    {
        private Queue<Flame> flames;
        private Vector3? LastPosition;
        private Vector3? LastDirection;
        public Vector3 Location { get; set; }

        public ThrustFlame()
        {
            flames = new Queue<Flame>();
        }

        public void UpdateThrust(Vector3 Position, Vector3 Direction, Vector3 Up, TimeSpan ElapsedTime, Environment Environment)
        {
            if (LastPosition.HasValue && LastDirection.HasValue)
            {
                var distance = Position - LastPosition.Value;
                var count = (Position - LastPosition.Value).Length() / 5;

                var forward = Vector3.Normalize(Direction);
                var up = Vector3.Normalize(Up);
                var left = Vector3.Normalize(Vector3.Cross(up, forward));
                Position += forward * Location.Z;
                Position += -left * Location.X;
                Position += up * Location.Y;

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