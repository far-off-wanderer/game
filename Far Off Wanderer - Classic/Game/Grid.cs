using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public class Grid
    {
        readonly float range;
        readonly int cellCount = 16;
        readonly float cellSize;
        readonly List<Object3D>[,] grid;
        DistanceField distanceField;
        Vector3 distanceFieldPosition;
        float distanceFieldSize;

        public Grid(float range)
        {
            this.range = range;

            cellSize = range / cellCount;
            grid = new List<Object3D>[cellCount, cellCount];
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z] = new List<Object3D>();
                }
            }

        }

        public void AddDistanceField(DistanceField distanceField, Vector3 position, float size)
        {
            this.distanceField = distanceField;
            this.distanceFieldPosition = position;
            this.distanceFieldSize = size;
        }

        public void SetCurrentColliders(IEnumerable<Object3D> collidableObjects)
        {
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    grid[x, z].Clear();
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
                        grid[Imod(x), Imod(z)].Add(object3d);
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
                    var objects = grid[x, z].ToArray();

                    if (objects.Length > 0)
                    {
                        for (int a = 0; a < objects.Length; a++)
                        {
                            var objectA = objects[a];

                            if(distanceField.DistanceAt((objectA.Position - distanceFieldPosition) / distanceFieldSize) * distanceFieldSize < objectA.Radius)
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
                    colliders.AddRange(grid[Imod(x), Imod(z)].OfType<Collider>());
                }
            }

            return colliders.Distinct();
        }
    }
}
