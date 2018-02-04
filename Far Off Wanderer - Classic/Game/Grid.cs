using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Grid
    {
        float range;

        int cellCount = 16;
        float cellSize;
        (Object3D[] staticColliders, List<Object3D> colliders)[,] grid;
        InfiniteTerrainDistanceField distanceField;

        public Grid(float range)
        {
            this.range = range;

            cellSize = range / cellCount;
            grid = new(Object3D[] staticColliders, List<Object3D> colliders)[cellCount, cellCount];
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z] = (null, new List<Object3D>());
                }
            }

        }

        public void AddDistanceField(InfiniteTerrainDistanceField distanceField)
        {
            this.distanceField = distanceField;
        }

        public void AddStaticColliders(IEnumerable<Collider> staticColliders)
        {
            foreach (var obj in staticColliders)
            {
                var minpos = (obj.Position - new Vector3(obj.Radius)) / cellSize;
                var maxpos = (obj.Position + new Vector3(obj.Radius)) / cellSize;
                
                for (var z = minpos.Z; z <= maxpos.Z; z++)
                {
                    for (var x = minpos.X; x <= maxpos.X; x++)
                    {
                        grid[Imod(x), Imod(z)].colliders.Add(obj);
                    }
                }
            }

            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z].staticColliders = grid[x, z].colliders.ToArray();
                    grid[x, z].colliders.Clear();
                    grid[x, z].colliders.Capacity = 0;
                }
            }
        }

        public void AddCurrentColliders(IEnumerable<Object3D> collidableObjects)
        {
            if(Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T))
            {
                var b = 0;
            }
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z].colliders.Clear();
                }
            }

            foreach (var object3d in collidableObjects)
            {
                var minpos = (object3d.Position - new Vector3(object3d.Radius)) / cellSize;
                var maxpos = (object3d.Position + new Vector3(object3d.Radius)) / cellSize;

                for (var z = minpos.Z; z <= maxpos.Z; z++)
                {
                    for (var x = minpos.X; x <= maxpos.X; x++)
                    {
                        grid[Imod(x), Imod(z)].colliders.Add(object3d);
                    }
                }
            }
        }

        int Imod(float a) => (int)Math.Floor(a - cellCount * Math.Floor(a / cellCount));

        Vector3 Vmod(Vector3 v) => new Vector3(
            (float)(v.X - range * Math.Floor(v.X / range)),
            (float)(v.Y - range * Math.Floor(v.Y / range)),
            (float)(v.Z - range * Math.Floor(v.Z / range))
        );

        public void CheckCollisions(Action<(Object3D obj, Vector3 at), (Object3D obj, Vector3 at)> interact)
        {
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    var objects = grid[x, z].colliders.ToArray();

                    if (objects.Length > 0)
                    {
                        for (int a = 0; a < objects.Length; a++)
                        {
                            var objectA = objects[a];

                            if(distanceField.DistanceAt(objectA.Position) < objectA.Radius)
                            {
                                interact((objectA, objectA.Position), (objectA, objectA.Position));
                                continue;
                            }

                            for (int b = a + 1; b < objects.Length; b++)
                            {
                                var objectB = objects[b];

                                var distance = objectB.Position - objectA.Position;
                                distance = Vmod(distance + new Vector3(range / 2)) - new Vector3(range / 2);

                                if (distance.LengthSquared() < (objectA.Radius + objectB.Radius) * (objectA.Radius + objectB.Radius))
                                {
                                    interact(
                                        (objectA, objectA.Position + distance / 2),
                                        (objectB, objectB.Position - distance / 2)
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<Collider> GetNearby(Vector3 position, float radius)
        {
            var minpos = (position - new Vector3(radius)) / cellSize;
            var maxpos = (position + new Vector3(radius)) / cellSize;

            var colliders = new List<Collider>();

            for (var z = minpos.Z; z <= maxpos.Z; z++)
            {
                for (var x = minpos.X; x <= maxpos.X; x++)
                {
                    colliders.AddRange(grid[Imod(x), Imod(z)].staticColliders.OfType<Collider>());
                }
            }

            return colliders.Distinct();
        }
    }
}
