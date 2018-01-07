using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conesoft.Game
{
    using Far_Off_Wanderer___Classic;

    public class DefaultLevel
    {
        public List<Player> Players { get; set; }
        public List<Object3D> Objects3D { get; set; }
        public Terrain Terrain { get; set; }
        public Skybox Skybox { get; set; }
        public Camera Camera { get; set; }
        public DefaultEnvironment Environment { get; set; }

        public DefaultLevel(DefaultEnvironment environment)
        {
            Environment = environment;

            Players = new List<Player>();
            Objects3D = new List<Object3D>();
            var random = new Random();
            int factor = 3;
            Objects3D.Add(new Spaceship()
            {
                Id = Data.Ship,
                Position = Vector3.Zero,
                Orientation = Quaternion.Identity,
                Speed = 150,
                Boundary = environment.ModelBoundaries[Data.Ship],
                ThrustFlame = new ThrustFlame()
                {
                    ThrustBackshift = 150
                }
            });
            for (int y = -factor; y <= factor; y++)
            {
                for (int x = -factor; x <= factor; x++)
                {
                    if (y != 0 || x != 0)
                    {
                        var ids = new string[] { Data.Ship, Data.Drone };
                        var id = ids[environment.Random.Next(0, ids.Length)];
                        var spaceship = new Spaceship()
                        {
                            Id = id,
                            Position = 6400 * (Vector3.Forward * y + Vector3.Right * x) * (float)factor * 2 / 5,
                            Orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, x + y),
                            Speed = id == ids[0] ? 150 : 2,
                            Boundary = environment.ModelBoundaries[id],
                            ThrustFlame = id == ids[1] ? null : new ThrustFlame()
                            {
                                ThrustBackshift = 150
                            }
                        };
                        Players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        Objects3D.Add(spaceship);
                    }
                }
            }
            Terrain = new Terrain();
            Skybox = new Skybox()
            {
                Color = new Color(0.2f, 0.3f, 0.8f)
            };
            if (Objects3D.Count > 0)
            {
                Camera = new SpaceshipFollowingCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 3,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Ship = Objects3D.OfType<Spaceship>().First()
                };
            }
            else
            {
                Camera = new FixedCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 4,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Position = Vector3.Backward * 2,
                    Target = Vector3.Zero
                };
            }
        }

        public void UpdateScene(TimeSpan ElapsedTime)
        {
            var newObjects = new List<Object3D>();
            foreach (var objects3d in Objects3D)
            {
                newObjects.AddRange(objects3d.Update(Environment, ElapsedTime));
            }

            foreach (var player in Players)
            {
                player.UpdateThinking(ElapsedTime, Environment);
            }

            var collidableObjects = (from object3d in Objects3D
                                     where object3d.Boundary != Object3D.EmptyBoundary
                                     select
                                     (
                                         position: object3d.Position,
                                         radius: object3d.Boundary.Radius,
                                         boundingSphere: new BoundingSphere(object3d.Position, object3d.Boundary.Radius),
                                         object3d: object3d
                                     )
                                    ).ToArray();

            var min = new Vector3(float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity);

            foreach (var obj in collidableObjects)
            {
                min.X = Math.Min(obj.position.X - obj.radius, min.X);
                min.Y = Math.Min(obj.position.Y - obj.radius, min.Y);
                min.Z = Math.Min(obj.position.Z - obj.radius, min.Z);
                max.X = Math.Max(obj.position.X + obj.radius, max.X);
                max.Y = Math.Max(obj.position.Y + obj.radius, max.Y);
                max.Z = Math.Max(obj.position.Z + obj.radius, max.Z);
            }

            max += Vector3.One;
            min -= Vector3.One;

            var cellCount = 48;
            var cellSize = (max - min) / cellCount;
            var grid = new List<Object3D>[cellCount, cellCount];
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z] = new List<Object3D>();
                }
            }

            foreach (var obj in collidableObjects)
            {
                var minpos = (obj.position - min - new Vector3(obj.radius)) / cellSize;
                var maxpos = (obj.position - min + new Vector3(obj.radius)) / cellSize;

                int f(float v) => (int)Math.Floor(v);

                for (var z = f(minpos.Z); z <= f(maxpos.Z); z++)
                {
                    for (var x = f(minpos.X); x <= f(maxpos.X); x++)
                    {
                        grid[x, z].Add(obj.object3d);
                    }
                }
            }


            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    var objects = grid[x, z].ToArray();

                    if (objects.Length > 1)
                    {
                        for (int a = 0; a < objects.Length; a++)
                        {
                            var objectA = objects[a];
                            var sphereA = new BoundingSphere(objectA.Position, objectA.Boundary.Radius);

                            for (int b = a + 1; b < objects.Length; b++)
                            {
                                var objectB = objects[b];
                                var sphereB = new BoundingSphere(objectB.Position, objectB.Boundary.Radius);

                                if (sphereA.Intersects(sphereB))
                                {
                                    var collisionPoint = (objectA.Position + objectB.Position) / 2;

                                    newObjects.AddRange(objectA.Die(Environment, collisionPoint));
                                    newObjects.AddRange(objectB.Die(Environment, collisionPoint));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var newObject in newObjects)
            {
                newObject.Update(Environment, ElapsedTime);
            }
            Objects3D.AddRange(newObjects);
            foreach (var deadObject3d in (from object3d in Objects3D where object3d.Alive == false select object3d).ToArray())
            {
                Objects3D.Remove(deadObject3d);
            }

            var localPlayers = Players.OfType<LocalPlayer>();
            if (localPlayers.Any())
            {
                var center = localPlayers.First().ControlledObject;
                var range = Environment.Range;
                foreach (var other in Objects3D)
                {
                    Vector3 distance()
                    {
                        return other.Position - center.Position;
                    }
                    while (distance().X > range / 2)
                    {
                        other.Position = other.Position - new Vector3(range, 0, 0);
                    }
                    while (distance().X < -range / 2)
                    {
                        other.Position = other.Position + new Vector3(range, 0, 0);
                    }
                    while (distance().Y > range / 2)
                    {
                        other.Position = other.Position - new Vector3(0, range, 0);
                    }
                    while (distance().Y < -range / 2)
                    {
                        other.Position = other.Position + new Vector3(0, range, 0);
                    }
                    while (distance().Z > range / 2)
                    {
                        other.Position = other.Position - new Vector3(0, 0, range);
                    }
                    while (distance().Z < -range / 2)
                    {
                        other.Position = other.Position + new Vector3(0, 0, range);
                    }
                }
            }
        }
    }
}
