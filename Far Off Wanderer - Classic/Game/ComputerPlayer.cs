using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class ComputerPlayer : Player
    {
        double time = 0;
        float[] directions = { -0.3f, 0.3f, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0 };
        int direction = 0;
        double shootTime = float.NegativeInfinity;
        bool shoot = false;

        float visibleRange = 175000;

        private (float lean, StrafingDirection? strafe) Look(Environment environment)
        {
            var my = ControlledObject;

            var forward = Vector3.Normalize(Vector3.Transform(Vector3.Forward, my.Orientation));
            var left = Vector3.Normalize(Vector3.Cross(Vector3.Up, forward));

            var field = environment.DistanceField;
            
            var sensorCount = 2;
            var sensors = Enumerable.Range(0, sensorCount).Select(i =>
            {
                var l = (i * 2) / (sensorCount - 1f) - 1;
                l = Math.Sign(l) * (float)Math.Pow(Math.Abs(l), 1);
                l *= .5f;
                return (distance: 0f, collision: my.Position, range: 0f, leftishness: l);
            }).ToArray();

            for(var i = 0; i < sensors.Length; i++)
            {
                var sensor = sensors[i];
                sensor.range = visibleRange * (float)Math.Exp(-Math.Abs(sensor.leftishness * 2));
                var direction = Vector3.Normalize(forward + sensor.leftishness * left);
                while (sensor.distance < sensor.range)
                {
                    var d = field.DistanceAt(sensor.collision);
                    if (d < my.Boundary.Radius * 10)
                    {
                        break;
                    }
                    sensor.distance += d;
                    sensor.distance = Math.Min(sensor.range, sensor.distance);
                    sensor.collision = my.Position + sensor.distance * direction;
                }
                sensors[i] = sensor;
            }
            
            var leftishness = 0f;

            foreach(var sensor in sensors)
            {
                leftishness -= sensor.leftishness * (sensor.distance < sensor.range * .7f ? 1 : 0);
            }

            var hits = sensors.Where(s => s.distance < s.range);
            if (hits.Any())
            {
                var nearest = hits.OrderBy(s => s.distance).First();
                leftishness = -(1 - nearest.distance / nearest.range) * nearest.leftishness;
            }
            else leftishness = 0;


            //leftishness = distanceForward > visibleRange * .99f ? 0 : leftishness;

            var ship = ControlledObject as Spaceship;
//            ship.SensorPoints = sensors.Select(s => s.collision).ToArray();

            //leftishness *= 1 - distanceForward / visibleRange;

            //leftishness = Math.Min(1, leftishness);

            //leftishness = Math.Sign(leftishness) * (float)Math.Pow(Math.Abs(leftishness), 2);


            if (sensors.Where(s => s.leftishness > 0).Any(s => s.distance < s.range / 10))
            {
                return (lean: 0f, strafe: StrafingDirection.Right);
            }
            else if (sensors.Where(s => s.leftishness < 0).Any(s => s.distance < s.range / 10))
            {
                return (lean: 0f, strafe: StrafingDirection.Left);
            }
            else
            {
                return (lean: -leftishness, strafe: null);
            }

            //var nearby = environment.Grid.GetNearby(my.Position, visibleRange);

            //var range = environment.Range;
            //Vector3 vmod(Vector3 v) => new Vector3(
            //    (float)(v.X - range * Math.Floor(v.X / range)),
            //    (float)(v.Y - range * Math.Floor(v.Y / range)),
            //    (float)(v.Z - range * Math.Floor(v.Z / range))
            //);

            //var classification = nearby.Select(o =>
            //{
            //    var to = o.Position - my.Position;
            //    to = vmod(to + new Vector3(range / 2)) - new Vector3(range / 2);

            //    var distance = to.Length();
            //    var direction = to / distance;
            //    var inView = Vector3.Dot(forward, direction) > 0.3;
            //    var leftishness = Vector3.Dot(left, direction);

            //    return new
            //    {
            //        to,
            //        distance = distance - (my.Boundary.Radius - o.Boundary.Radius),
            //        direction,
            //        inView,
            //        leftishness
            //    };
            //});

            //var interpretation = classification
            //    .Where(element => element.inView && element.distance < visibleRange)
            //    .Sum(element => element.leftishness * 1 / element.distance);

            //if (classification.Any(element => element.distance < visibleRange / 3))
            //{
            //    // panic mode
            //    var nearest = classification.OrderBy(element => element.distance).First();
            //    return (lean: 0f, strafe: nearest.leftishness > 0 ? StrafingDirection.Right : StrafingDirection.Left);
            //}

            //return (lean: interpretation, strafe: null);
        }

        public override void UpdateThinking(TimeSpan timeSpan, Environment environment)
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
                //if (Math.Abs(lean) > 0.0001)
                {
                    spaceShip.TurnAngle(-lean * 15);
                }
                //else
                //{
                //    spaceShip.TurnAngle(directions[direction]);
                //}
            }
        }
    }
}
