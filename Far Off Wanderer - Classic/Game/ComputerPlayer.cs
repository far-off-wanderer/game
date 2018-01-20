using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conesoft.Game
{
    public class ComputerPlayer : Player
    {
        double time = 0;
        float[] directions = { -0.3f, 0.3f, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0 };
        int direction = 0;
        double shootTime = float.NegativeInfinity;
        bool shoot = false;

        float visibleRange = 35000;

        private (float lean, StrafingDirection? strafe) Look(DefaultEnvironment environment)
        {
            var my = ControlledObject;

            var forward = Vector3.Transform(Vector3.Forward, my.Orientation);
            var left = Vector3.Cross(Vector3.Up, forward);

            var nearby = environment.Grid.GetNearby(my.Position, visibleRange);

            var range = environment.Range;
            Vector3 vmod(Vector3 v) => new Vector3(
                (float)(v.X - range * Math.Floor(v.X / range)),
                (float)(v.Y - range * Math.Floor(v.Y / range)),
                (float)(v.Z - range * Math.Floor(v.Z / range))
            );

            var classification = nearby.Select(o =>
            {
                var to = o.Position - my.Position;
                to = vmod(to + new Vector3(range / 2)) - new Vector3(range / 2);

                var distance = to.Length();
                var direction = to / distance;
                var inView = Vector3.Dot(forward, direction) > 0.3;
                var leftishness = Vector3.Dot(left, direction);

                return new
                {
                    to,
                    distance = distance - (my.Boundary.Radius - o.Boundary.Radius),
                    direction,
                    inView,
                    leftishness
                };
            });

            var boops = classification.ToArray();

            var interpretation = classification
                .Where(element => element.inView && element.distance < visibleRange)
                .Sum(element => element.leftishness * 1 / element.distance);

            if(classification.Any(element => element.distance < visibleRange / 10))
            {
                // panic mode
                var nearest = classification.OrderBy(element => element.distance).First();
                return (lean: 0f, strafe: nearest.leftishness > 0 ? StrafingDirection.Right : StrafingDirection.Left);
            }

            return (lean: interpretation, strafe: null);
        }

        public override void UpdateThinking(TimeSpan timeSpan, DefaultEnvironment environment)
        {
            if (shootTime == float.NegativeInfinity)
            {
                shootTime = -3 - environment.Random.NextDouble() * 3;
            }
            time += timeSpan.TotalSeconds;
            shootTime += timeSpan.TotalSeconds;
            var next = environment.Random.NextDouble() * direction + 0.5;
            if (time > next)
            {
                time -= next;
                direction = environment.Random.Next(directions.Length);
            }
            var nextShoot = environment.Random.NextDouble() * (shoot ? 0.25 : 1) + (shoot ? 0.25 : 2);
            if (shootTime > nextShoot)
            {
                shootTime -= nextShoot;
                shoot = !shoot;
            }
            var spaceShip = ControlledObject as Spaceship;
            if (shoot)
            {
                spaceShip.Shoot();
            }

            var decision = Look(environment);
            if (decision.strafe.HasValue)
            {
                spaceShip.Strafe(decision.strafe.Value);
            }
            else
            {
                var lean = decision.lean;
                if (Math.Abs(lean) > 0.0001)
                {
                    spaceShip.TurnAngle(-lean * 15);
                }
                else
                {
                    spaceShip.TurnAngle(directions[direction]);
                }
            }
        }
    }
}
