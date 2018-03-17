using Microsoft.Xna.Framework;
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
        public LocalPlayer LocalPlayer => players.OfType<LocalPlayer>().First();

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
            int factor = 3;
            objects3D.Add(new Spaceship(
                id: Data.Ship,
                position: Vector3.Zero,
                orientation: Quaternion.Identity,
                speed: 150,
                radius: environment.ModelBoundaries[Data.Ship].Radius
            ));
            for (int y = -factor; y <= factor; y++)
            {
                for (int x = -factor; x <= factor; x++)
                {
                    if (y != 0 || x != 0)
                    {
                        var ids = new string[] { Data.Ship, Data.Drone };
                        var id = ids[environment.Random.Next(0, ids.Length)];
                        var spaceship = new Spaceship(
                            id: id,
                            position: (9400 * (Vector3.Forward * y + Vector3.Right * x) * factor) * 2 / 5,
                            orientation: Quaternion.CreateFromAxisAngle(Vector3.Up, x - y),
                            speed: id == ids[0] ? 150 : 2,
                            radius: environment.ModelBoundaries[id].Radius
                        );
                        players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        objects3D.Add(spaceship);
                    }
                }
            }
            //Objects3D.RemoveRange(2, Objects3D.Count - 2);
            skybox = new Skybox(
//                color: new Color(0.2f, 0.3f, 0.8f)
                color: new Color(0.9f, 0.7f, 0.4f)
            );
            if (objects3D.Count > 0)
            {
                camera = new SpaceshipFollowingCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 3,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Ship = objects3D.OfType<Spaceship>().First()
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

            if(ElapsedTime == TimeSpan.Zero)
            {
                return;
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
                                     where object3d.Radius > 0
                                     select object3d);

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
            objects3D = objects3D.Where(obj => obj.Alive == true).ToList();

            var center = camera.Position;
            var range = environment.Range;
            foreach (var other in objects3D)
            {
                var position = other.Position;
                while (position.X - center.X > range / 2)
                {
                    position.X -= range;
                }
                while (position.X - center.X < -range / 2)
                {
                    position.X += range;
                }
                while (position.Y - center.Y > range / 2)
                {
                    position.Y -= range;
                }
                while (position.Y - center.Y < -range / 2)
                {
                    position.Y += range;
                }
                while (position.Z - center.Z > range / 2)
                {
                    position.Z -= range;
                }
                while (position.Z - center.Z < -range / 2)
                {
                    position.Z += range;
                }
                other.Position = position;
            }
        }
    }
}
