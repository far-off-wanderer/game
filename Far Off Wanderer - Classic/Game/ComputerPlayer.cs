using System;
using System.Collections.Generic;

namespace Conesoft.Game
{
    public class ComputerPlayer : Player
    {
        double time = 0;
        float[] directions = { -0.3f, 0.3f, 0.2f, -0.2f, -0.1f, 0.1f, 0, 0, 0, 0, 0 };
        int direction = 0;
        double shootTime = float.NegativeInfinity;
        bool shoot = false;

        public override void UpdateThinking(TimeSpan timeSpan, DefaultEnvironment environment)
        {
            if (shootTime == float.NegativeInfinity)
            {
                shootTime = -3 -environment.Random.NextDouble() * 3;
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
            spaceShip.TurnAngle(directions[direction]);
        }
    }
}
