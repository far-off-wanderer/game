using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Spaceship : ControllableObject3D
    {
        ThrustFlame leftFlame;
        ThrustFlame rightFlame;

        float speed;

        float rotation;
        float maxRotation = (float)(Math.PI / 2) / 25;
        float rotationSpeed = 0.15f;

        float targetRotation;
        float forwardAcceleration;

        bool readyToShoot;
        bool shooting;
        float lastShot;
        float shotTrigger = 0.1f;

        bool beginStrafing;
        StrafingDirection? strafe;
        float strafingTime = 0.25f;
        float strafingIdle = 1 - 0.25f;
        float strafingRatio = 2f;
        float strafing;
        
        public Vector3[] SensorPoints { get; set; }

        public float Speed => speed;
        public Quaternion ShipLeaning => Quaternion.CreateFromAxisAngle(Vector3.Forward, -rotation * 5); // the @buildstarted factor
        public float StrafingAngle => (strafe == StrafingDirection.Left ? 1 : -1) * MathHelper.SmoothStep(0, 2 * (float)Math.PI, strafing);
        public Quaternion Strafing => Quaternion.CreateFromAxisAngle(Vector3.Forward, StrafingAngle);
        public Vector3 Forward => Vector3.Transform(Vector3.Forward, Orientation);
        public Vector3 Up => Vector3.Transform(Vector3.Up, ShipLeaning * Strafing);
        public Vector3 Right => Vector3.Cross(Forward, Up);

        float StrafingAmount => MathHelper.SmoothStep(0, 1, 1 - Math.Abs(1 - Math.Max(0, strafing) * 2));
        Quaternion Rotation => Quaternion.CreateFromAxisAngle(Vector3.Up, rotation);

        public bool IsStafing => strafe.HasValue;

        void UpdateCanon(TimeSpan ElapsedTime)
        {
            lastShot -= (float)ElapsedTime.TotalSeconds;
            if (lastShot < -shotTrigger)
            {
                lastShot = 0;
                readyToShoot = true;
            }
        }

        public Spaceship(string id, Vector3 position, Quaternion orientation, float speed, float radius)
        {
            this.Id = id;
            this.Position = position;
            this.Orientation = orientation;
            this.speed = speed;
            this.Radius = radius;

            this.lastShot = -shotTrigger;

            this.leftFlame = new ThrustFlame
            {
                Location = new Vector3(-20, 3, 20)
            };
            this.rightFlame = new ThrustFlame
            {
                Location = new Vector3(20, 3, 20)
            };

        }

        public override IEnumerable<Object3D> Update(Environment Environment, TimeSpan ElapsedTime)
        {
            UpdateCanon(ElapsedTime);

            if (readyToShoot && shooting)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                //Environment.Sounds[Data.LaserSound].Play(1 / (1 + dst / 5000), 0, 0);
                var bulletDirection = Vector3.Normalize(Vector3.Transform(Vector3.Forward, Orientation));
                var bullet = new Bullet(Position, bulletDirection, Speed * 100 + 6250);
                bullet.Position += (bullet.Radius + Radius) * 2 * bulletDirection;
                yield return bullet;

                readyToShoot = false;
            }
            shooting = false;

            speed += forwardAcceleration * (float)ElapsedTime.TotalSeconds;
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

            if(beginStrafing == true)
            {
                beginStrafing = false;
            }

            if (strafe.HasValue && strafing < -strafingIdle)
            {
                strafing = 1;
                beginStrafing = true;
            }
            else if (strafing >= -strafingIdle)
            {
                var strafeDirection = Vector3.Normalize(Vector3.Cross(Up, Direction));
                if (strafe == StrafingDirection.Right)
                {
                    strafeDirection = -strafeDirection;
                }
                Position += strafeDirection * strafingRatio * StrafingAmount * this.Radius;

                strafing -= (float)ElapsedTime.TotalSeconds / strafingTime;
                if (strafing < -strafingIdle)
                {
                    strafe = null;
                }
            }


            if (SensorPoints != null)
            {
                foreach (var point in SensorPoints)
                {
                    for (var i = 0f; i < 1f; i += .05f)
                    {
                        yield return new Explosion(
                            id: Data.Sparkle,
                            position: Vector3.Lerp(Position, point, i),
                            endOfLife: 0.001f,
                            minSize: 100,
                            maxSize: 100,
                            startSpin: 0,
                            spin: 0
                        );
                    }
                }
            }

            foreach (var thrustParticle in GenerateThrust(Environment, ElapsedTime))
            {
                yield return thrustParticle;
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
            shooting = true;
        }

        private IEnumerable<Object3D> GenerateThrust(Environment Environment, TimeSpan ElapsedTime)
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
                IEnumerable<Explosion> createThrustFlames(ThrustFlame flame, float scale)
                {
                    var blend = 1f / flame.Flames.Count;
                    while (flame.Flames.Count > 0)
                    {
                        var position = flame.Flames.Dequeue();
                        var lifeTime = LifeTimes[Environment.Random.Next(LifeTimes.Length)];
                        yield return new Explosion(
                            id: Data.Sparkle,
                            position: position.Position,
                            endOfLife: lifeTime / 5,
                            minSize: scale * MathHelper.Lerp(50, 25, flame.Flames.Count * blend) * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            maxSize: 10 * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            startSpin: 20 * (float)(Environment.Random.NextDouble() * 10 - 5),
                            spin: 200
                        );
                    }
                }
                IEnumerable<Object3D> createRollFlames()
                {
                    var rollFlames = MathHelper.Lerp(150, -50, StrafingAmount);
                    var rollSpeed = MathHelper.Lerp(1, 50, StrafingAmount);
                    for (var i = 0; i < rollFlames; i++)
                    {
                        var lifeTime = LifeTimes[Environment.Random.Next(LifeTimes.Length)];
                        var explosion = new Explosion(
                            id: Data.Sparkle,
                            position: Position + Environment.RandomDirection() * Radius * .8f,
                            endOfLife: lifeTime * .125f,
                            minSize: MathHelper.Lerp(40, 25, i / rollFlames) * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            maxSize: MathHelper.Lerp(40, 25, i / rollFlames) * ((0.1f / lifeTime) * 0.2f + 0.8f),
                            startSpin: 20 * (float)(Environment.Random.NextDouble() * 10 - 5),
                            spin: 200
                        );
                        yield return explosion;
                        yield return new Orbit(this, explosion, strafe == StrafingDirection.Left ? -rollSpeed : rollSpeed);
                    }
                }
                foreach (var explosion in createThrustFlames(leftFlame, (float)Math.Exp(-10 * rotation)))
                {
                    yield return explosion;
                }
                foreach (var explosion in createThrustFlames(rightFlame, (float)Math.Exp(10 * rotation)))
                {
                    yield return explosion;
                }
                if (strafe.HasValue)
                {
                    foreach (var object3d in createRollFlames())
                    {
                        yield return object3d;
                    }
                }
            }
        }

        public override IEnumerable<Explosion> Die(Environment Environment, Vector3 CollisionPoint)
        {
            if (Alive == true)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                //Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
                //Environment.TriggerVibration(1 / (1 + dst));
            }
            Alive = false;
            yield return new Explosion(
                id: Data.Fireball,
                position: CollisionPoint,
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
