using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Level
    {
        public Camera Camera { get; }

        public IEnumerable<Object3D> Objects3D => worldSimulation.Objects3D;
        public LocalPlayer LocalPlayer => players.OfType<LocalPlayer>().First();

        readonly Environment environment;

        List<Player> players;

        WorldSimulation worldSimulation;

        public Level(Environment environment, Dictionary<string, float> modelBoundaries, IEnumerable<Landscape> landscapes)
        {
            this.environment = environment;
            this.worldSimulation = new WorldSimulation();

            players = new List<Player>();
            var random = new Random();
            int factor = 1;
            
            worldSimulation.Add(new Spaceship(
                id: Data.Ship,
                position: Vector3.Zero,
                horizontalOrientation: 0f,
                speed: 150,
                radius: modelBoundaries[Data.Ship]
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
                            horizontalOrientation: x - y,
                            speed: id == ids[0] ? 150 : 2,
                            radius: modelBoundaries[id]
                        );
                        players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        worldSimulation.Add(spaceship);
                    }
                }
            }

            worldSimulation.Add(landscapes);

            if (worldSimulation.IsEmpty)
            {
                Camera = new FixedCamera()
                {
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 4,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Position = Vector3.Backward * 2,
                    Target = Vector3.Zero
                };
            }
            else
            {
                Camera = new SpaceshipFollowingCamera()
                {
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 3,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Ship = worldSimulation.GetAll<Spaceship>().First()
                };
            }
            players.Add(new LocalPlayer()
            {
                ControlledObject = worldSimulation.GetAll<Spaceship>().First()
            });
        }

        public bool NoLocalPlayerLeft => Objects3D.Contains(LocalPlayer.ControlledObject) == false;
        public IEnumerable<Object3D> Enemies => Objects3D.Except(LocalPlayer.ControlledObject).OfType<Spaceship>();
        public bool NoEnemiesLeft => !Enemies.Any();

        public void UpdateScene(TimeSpan elapsedTime, LevelHandler.InputActions actions)
        {
            if (Camera is SpaceshipFollowingCamera)
            {
                var camera = this.Camera as SpaceshipFollowingCamera;
                var ship = camera.Ship;
                camera.Up = Vector3.Normalize(Vector3.Up * 2 + ship.Up); // needs to be better.. 
                camera.Yaw = actions.CameraYaw;
                camera.Pitch = actions.CameraPitch;
                camera.ZoomIn = actions.ZoomingIn;
            }

            if(elapsedTime == TimeSpan.Zero)
            {
                return;
            }

            worldSimulation.Update(elapsedTime, environment);

            foreach (var player in players)
            {
                player.UpdateThinking(elapsedTime, environment);
            }

        }
    }
}
