namespace Far_Off_Wanderer.Game;

using System;
using System.Linq;

public class ComputerPlayer : Player
{
    double time = 0;
    float[] directions = { -0.3f, 0.3f, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0 };
    int direction = 7;
    double shootTime = float.NegativeInfinity;
    bool shoot = false;
    readonly float defaultVisibleRange = 67500;

    private (float lean, StrafingDirection? strafe) Look(Environment environment)
    {
        var my = ControlledObject as Spaceship;
        var visibleRange = defaultVisibleRange * my.Speed / 150;

        var forward = my.Forward;
        var left = -my.Right;

        //var field = environment.DistanceField;

        var sensorCount = 2;
        var sensors = Enumerable.Range(0, sensorCount).Select(i =>
        {
            var l = i * 2 / (sensorCount - 1f) - 1;
            l = Math.Sign(l) * (float)Math.Pow(Math.Abs(l), 4);
            l *= .5f;
            return (distance: 0f, collision: my.Position, range: 0f, leftishness: l);
        }).ToArray();

        //for(var i = 0; i < sensors.Length; i++)
        //{
        //    var sensor = sensors[i];
        //    sensor.range = visibleRange * (float)Math.Exp(-Math.Abs(sensor.leftishness * 5));
        //    var direction = Vector3.Normalize(forward + sensor.leftishness * left);
        //    while (sensor.distance < sensor.range)
        //    {
        //        var d = field.DistanceAt(sensor.collision);
        //        if (d < my.Radius)
        //        {
        //            break;
        //        }
        //        sensor.distance += d;
        //        sensor.distance = Math.Min(sensor.range, sensor.distance);
        //        sensor.collision = my.Position + sensor.distance * direction;
        //    }
        //    sensors[i] = sensor;
        //}

        var leftishness = 0f;

        foreach (var sensor in sensors)
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

        //var ship = ControlledObject as Spaceship;
        //ship.SensorPoints = sensors.Select(s => s.collision).ToArray();

        var panic = sensors.Any(s => s.distance < s.range / 10);


        if (panic)
        {
            return (lean: 0f, strafe: leftishness > 0 ? StrafingDirection.Left : StrafingDirection.Right);
        }
        else
        {
            return (lean: -leftishness, strafe: null);
        }
    }

    public override void UpdateThinking(TimeSpan timeSpan, Environment environment)
    {
        if (shootTime == float.NegativeInfinity)
        {
            shootTime = -3 - environment.Random.Real.UpTo(3);
        }
        time += timeSpan.TotalSeconds;
        shootTime += timeSpan.TotalSeconds;
        var next = environment.Random.Real.Between(2.5f, 6.5f);
        if (time > next)
        {
            time -= next;
            direction = environment.Random.Integer.UpTo(directions.Length);
        }
        var nextShoot = shoot ? environment.Random.Real.Between(.25f, .5f) : environment.Random.Real.Between(2f, 3f);
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
        if (decision.strafe.HasValue && !spaceShip.IsStafing)
        {
            spaceShip.Strafe(decision.strafe.Value);
        }
        else if (spaceShip.IsStafing == false)
        {
            var lean = decision.lean;
            if (Math.Abs(lean) > 0.0001)
            {
                spaceShip.HorizontalTurnAngle(-lean * 15);
            }
            else
            {
                spaceShip.HorizontalTurnAngle(directions[direction]);
            }
        }
    }
}
