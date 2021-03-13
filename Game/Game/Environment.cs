using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Drawing;
using XNA = Microsoft.Xna.Framework.Input;

namespace Far_Off_Wanderer
{
    public class Environment
    {
        public Size ScreenSize { get; set; }
        public bool Flipped { get; set; }
        public Random Random { get; set; }
        public Dictionary<string, SoundEffect> Sounds { get; set; }
        public Camera ActiveCamera { get; set; }
        public float Range { get; set; }
        public LevelHandler.InputActions Actions { get; set; }

        public Vector3 RandomDirection()
        {
            return Vector3.Normalize(RandomPointInUnitSphere());
        }

        public Vector3 RandomPointInUnitSphere()
        {
            var direction = Vector3.Zero;
            do
            {
                direction.X = (float)(Random.NextDouble() * 2 - 1);
                direction.Y = (float)(Random.NextDouble() * 2 - 1);
                direction.Z = (float)(Random.NextDouble() * 2 - 1);
            } while (direction.LengthSquared() > 1);
            return direction;
        }

        public Vector2 RandomPointInUnitCircle()
        {
            var direction = Vector2.Zero;
            do
            {
                direction.X = (float)(Random.NextDouble() * 2 - 1);
                direction.Y = (float)(Random.NextDouble() * 2 - 1);
            } while (direction.LengthSquared() > 1);
            return direction;
        }

        float vibration = 0f;

        public void Update(TimeSpan timeSpan)
        {
            if (vibration > 0)
            {
                vibration -= (float)timeSpan.TotalSeconds * 1.2f;
//                GamePad.SetVibration(PlayerIndex.One, vibration * vibration, vibration * vibration);
            } else
            {
                XNA.GamePad.SetVibration(PlayerIndex.One, 0, 0);
            }
        }

        public void TriggerVibration(float strength)
        {
            vibration += strength;
            vibration = Math.Max(1, vibration);
        }
    }
}
