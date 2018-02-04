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

        Grid grid;
        
        public DefaultLevel(DefaultEnvironment environment)
        {
            Environment = environment;

            Players = new List<Player>();
            Objects3D = new List<Object3D>();
            var random = new Random();
            int factor = 1;
            Objects3D.Add(new Spaceship()
            {
                Id = Data.Ship,
                Position = Vector3.Zero,
                Orientation = Quaternion.Identity,
                Speed = 150,
                Boundary = environment.ModelBoundaries[Data.Ship]
            });
            for (int y = -factor; y <= factor; y++)
            {
                for (int x = -factor; x <= factor; x++)
                {
                    if (y != 0 || x != 0)
                    {
                        var ids = new string[] { Data.Ship, Data.Drone };
                        var id = ids[environment.Random.Next(0, ids.Length)];
                        id = ids.First();
                        var spaceship = new Spaceship()
                        {
                            Id = id,
                            Position = 6400 * (Vector3.Forward * y + Vector3.Right * x) * (float)factor * 2 / 5,
                            Orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, x - y),
                            Speed = id == ids[0] ? 150 : 2,
                            Boundary = environment.ModelBoundaries[id]
                        };
                        Players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        Objects3D.Add(spaceship);
                    }
                }
            }
            //Objects3D.RemoveRange(2, Objects3D.Count - 2);
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
                    Ship = Objects3D.OfType<Spaceship>().Skip(1).First()
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

            grid = new Grid(Environment.Range);

            grid.AddStaticColliders(environment.StaticColliders);
            grid.AddDistanceField(environment.DistanceField);

            environment.Grid = grid;
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

            grid.AddCurrentColliders(collidableObjects);

            grid.CheckCollisions((a, b) =>
            {
                newObjects.AddRange(a.obj.Die(Environment, a.at));
                newObjects.AddRange(b.obj.Die(Environment, b.at));
            });

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
                var center = Camera;
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
