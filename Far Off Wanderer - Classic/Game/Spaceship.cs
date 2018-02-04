using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public class Spaceship : ControllableObject3D
    {
        public ThrustFlame leftFlame;
        public ThrustFlame rightFlame;

        public Quaternion ShipLeaning
        {
            get
            {
                return Quaternion.CreateFromAxisAngle(Vector3.Forward, -rotation * 5); // the @buildstarted factor
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return Quaternion.CreateFromAxisAngle(Vector3.Up, rotation);
            }
        }

        public Vector3[] SensorPoints { get; set; }

        public float Speed { get; set; }

        private float rotation = 0;
        private float maxRotation = (float)(Math.PI / 2) / 25;
        private float rotationSpeed = 0.15f;

        private float targetRotation = 0;
        private float forwardAcceleration = 0;

        private bool ReadyToShoot { get; set; }
        public bool Shooting { get; set; }
        private float lastShot = 0;
        private float shotTrigger = 0.1f;

        private StrafingDirection? strafe;
        private float strafingTime = 0.25f;
        private float strafingIdle = 1 - 0.25f;
        private float strafingRatio = 2f;
        private float strafing;
        public float StrafingAmount => MathHelper.SmoothStep(0, 1, 1 - Math.Abs(1 - Math.Max(0, strafing) * 2));
        public Quaternion Strafing => Quaternion.CreateFromAxisAngle(Vector3.Forward, (strafe == StrafingDirection.Left ? 1 : -1) * MathHelper.SmoothStep(0, 2 * (float)Math.PI, strafing));

        private void UpdateCanon(TimeSpan ElapsedTime)
        {
            lastShot -= (float)ElapsedTime.TotalSeconds;
            if (lastShot < -shotTrigger)
            {
                lastShot = 0;
                ReadyToShoot = true;
            }
        }

        public Spaceship()
        {
            leftFlame = new ThrustFlame
            {
                Location = new Vector3(-20, 3, 20)
            };
            rightFlame = new ThrustFlame
            {
                Location = new Vector3(20, 3, 20)
            };
            Id = Data.Spaceship;
            lastShot = -shotTrigger;
        }

        public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            UpdateCanon(ElapsedTime);

            if (ReadyToShoot && Shooting)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.LaserSound].Play(1 / (1 + dst / 5000), 0, 0);
                var bulletDirection = Vector3.Normalize(Vector3.Transform(Vector3.Forward, Orientation));
                var bullet = new Bullet(Position, bulletDirection, Speed * 100 + 6250);
                bullet.Position += (bullet.Boundary.Radius + Boundary.Radius) * 2 * bulletDirection;
                yield return bullet;

                ReadyToShoot = false;
            }
            Shooting = false;

            foreach (var thrustParticle in GenerateThrust(Environment, ElapsedTime))
            {
                yield return thrustParticle;
            }

            if(SensorPoints != null)
            {
                foreach (var point in SensorPoints)
                {
                    for(var i = 0f; i < 1f; i+= .05f)
                    {
                        yield return new Explosion(Data.Sparkle)
                        {
                            Position = Vector3.Lerp(Position, point, i),
                            EndOfLife = 0.001f,
                            MinSize = 100f,
                            MaxSize = 100f
                        };
                    }
                }
            }

            Speed += forwardAcceleration * (float)ElapsedTime.TotalSeconds;
            if (targetRotation > rotation)
            {
                rotation += rotationSpeed * (float)ElapsedTime.TotalSeconds;
                if (rotation >= targetRotation)
                {
                    rotation = targetRotation;
                }
            }
            else if (targetRotation < rotation)
            {
                rotation -= rotationSpeed * (float)ElapsedTime.TotalSeconds;
                if (rotation <= targetRotation)
                {
                    rotation = targetRotation;
                }
            }
            if (rotation > maxRotation)
            {
                rotation = maxRotation;
            }
            if (rotation < -maxRotation)
            {
                rotation = -maxRotation;
            }

            Orientation *= Rotation;
            var Direction = Vector3.Transform(Vector3.Forward * Speed, Orientation);
            var Up = Vector3.Transform(Vector3.Up, Orientation);
            Position += Direction * (ElapsedTime == TimeSpan.Zero ? 0 : 1);

            if (strafe.HasValue && strafing < -strafingIdle)
            {
                strafing = 1;
            }
            if (strafing >= -strafingIdle)
            {
                var strafeDirection = Vector3.Normalize(Vector3.Cross(Up, Direction));
                if (strafe == StrafingDirection.Right)
                {
                    strafeDirection = -strafeDirection;
                }
                Position += strafeDirection * strafingRatio * StrafingAmount * this.Boundary.Radius;

                strafing -= (float)ElapsedTime.TotalSeconds / strafingTime;
                if (strafing < -strafingIdle)
                {
                    strafe = null;
                }
            }

            if (leftFlame != null && rightFlame != null)
            {
                if (Speed > 0 && strafe.HasValue == false)
                {
                    leftFlame.UpdateThrust(Position, Direction, Up, ElapsedTime, Environment);
                    rightFlame.UpdateThrust(Position, Direction, Up, ElapsedTime, Environment);
                }
                else
                {
                    leftFlame.DontThrust();
                    rightFlame.DontThrust();
                }
            }
            yield break;
        }

        public override void TurnAngle(float Angle)
        {
            targetRotation = Angle;
        }

        public override void AccelerateAmount(float Amount)
        {
            forwardAcceleration = Amount;
        }

        public override void Strafe(StrafingDirection direction)
        {
            strafe = direction;
        }

        public override void Shoot()
        {
            Shooting = true;
        }

        private IEnumerable<Explosion> GenerateThrust(Environment Environment, TimeSpan ElapsedTime)
        {
            if (leftFlame != null && rightFlame != null)
            {
                var LifeTimes = new float[]
                {
                    0.1f, 0.1f, 0.1f, 0.1f,
                    0.1f, 0.1f, 0.1f, 0.1f,
                    0.2f, 0.2f, 0.3f, 0.3f,
                    1.5f
                };
                IEnumerable<Explosion> createExplosions(ThrustFlame flame, float scale)
                {
                    var blend = 1f / flame.Flames.Count;
                    while (flame.Flames.Count > 0)
                    {
                        var position = flame.Flames.Dequeue();
                        var lifeTime = LifeTimes[Environment.Random.Next(LifeTimes.Length)];
                        yield return new Explosion(Data.Sparkle)
                        {
                            Position = position.Position,
                            EndOfLife = lifeTime / 5,
                            MinSize = scale * MathHelper.Lerp(50, 25, flame.Flames.Count * blend) * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            MaxSize = 10 * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            StartSpin = 20 * (float)(Environment.Random.NextDouble() * 10 - 5),
                            Spin = 200
                        };
                    }
                }
                foreach (var explosion in createExplosions(leftFlame, (float)Math.Exp(-10 * rotation)))
                {
                    yield return explosion;
                }
                foreach (var explosion in createExplosions(rightFlame, (float)Math.Exp(10 * rotation)))
                {
                    yield return explosion;
                }
            }
        }

        public override IEnumerable<Explosion> Die(Environment Environment, Vector3 CollisionPoint)
        {
            if (Alive == true)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
                Environment.TriggerVibration(1 / (1 + dst));
            }
            Alive = false;
            yield return new Explosion(Data.Fireball)
            {
                Position = CollisionPoint,
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
