﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Level
    {
        public Camera Camera => camera;
        public Skybox Skybox => skybox;
        public IEnumerable<Object3D> Objects3D => objects3D;

        Environment environment;
        Grid grid;
        Camera camera;
        List<Player> players;
        List<Object3D> objects3D;
        Skybox skybox;

        public Level(Environment environment)
        {
            this.environment = environment;

            players = new List<Player>();
            objects3D = new List<Object3D>();
            var random = new Random();
            int factor = 1;
            objects3D.Add(new Spaceship()
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
                        players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        objects3D.Add(spaceship);
                    }
                }
            }
            //Objects3D.RemoveRange(2, Objects3D.Count - 2);
            skybox = new Skybox()
            {
                Color = new Color(0.2f, 0.3f, 0.8f)
            };
            if (objects3D.Count > 0)
            {
                camera = new SpaceshipFollowingCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 3,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Ship = objects3D.OfType<Spaceship>().Skip(1).First()
                };
            }
            else
            {
                camera = new FixedCamera()
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
            players.Add(new LocalPlayer()
            {
                ControlledObject = objects3D.OfType<Spaceship>().First()
            });


            grid = new Grid(this.environment.Range);

            grid.AddStaticColliders(environment.StaticColliders);
            grid.AddDistanceField(environment.DistanceField);

            environment.Grid = grid;
        }

        public void UpdateScene(TimeSpan ElapsedTime, float Yaw, float Pitch, bool ZoomIn)
        {
            if (camera is SpaceshipFollowingCamera)
            {
                var camera = this.camera as SpaceshipFollowingCamera;
                camera.Yaw = Yaw;
                camera.Pitch = Pitch;
                camera.ZoomIn = ZoomIn;
            }

            var newObjects = new List<Object3D>();
            foreach (var objects3d in objects3D)
            {
                newObjects.AddRange(objects3d.Update(environment, ElapsedTime));
            }

            foreach (var player in players)
            {
                player.UpdateThinking(ElapsedTime, environment);
            }

            var collidableObjects = (from object3d in objects3D
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
                newObjects.AddRange(a.obj.Die(environment, a.at));
                newObjects.AddRange(b.obj.Die(environment, b.at));
            });

            foreach (var newObject in newObjects)
            {
                newObject.Update(environment, ElapsedTime);
            }
            objects3D.AddRange(newObjects);
            foreach (var deadObject3d in (from object3d in objects3D where object3d.Alive == false select object3d).ToArray())
            {
                objects3D.Remove(deadObject3d);
            }

            var localPlayers = players.OfType<LocalPlayer>();
            if (localPlayers.Any())
            {
                var center = camera;
                var range = environment.Range;
                foreach (var other in objects3D)
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
