﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conesoft.Game
{
    using Conesoft.Engine;

    public class Grid
    {
        float range;

        int cellCount = 64;
        float cellSize;
        (Object3D[] staticColliders, List<Object3D> colliders)[,] grid;

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

        public void AddStaticColliders(IEnumerable<Collider> staticColliders)
        {
            foreach (var obj in staticColliders)
            {
                var minpos = (obj.Position - new Vector3(obj.Boundary.Radius)) / cellSize;
                var maxpos = (obj.Position + new Vector3(obj.Boundary.Radius)) / cellSize;
                
                for (var z = minpos.Z; z <= maxpos.Z; z++)
                {
                    for (var x = minpos.X; x <= maxpos.X; x++)
                    {
                        grid[imod(x), imod(z)].colliders.Add(obj);
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

        public void AddCurrentColliders((Vector3 position, float radius, BoundingSphere boundingSphere, Object3D object3d)[] collidableObjects)
        {
            for (var z = 0; z < cellCount; z++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    var cell = grid[x, z];
                    cell.colliders.Clear();
                }
            }

            foreach (var obj in collidableObjects)
            {
                var minpos = (obj.position - new Vector3(obj.radius)) / cellSize;
                var maxpos = (obj.position + new Vector3(obj.radius)) / cellSize;

                for (var z = minpos.Z; z <= maxpos.Z; z++)
                {
                    for (var x = minpos.X; x <= maxpos.X; x++)
                    {
                        grid[imod(x), imod(z)].colliders.Add(obj.object3d);
                    }
                }
            }
        }

        int imod(float a) => (int)Math.Floor(a - cellCount * Math.Floor(a / cellCount));

        Vector3 vmod(Vector3 v) => new Vector3(
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
                    var objects = grid[x, z].staticColliders.Concat(grid[x, z].colliders).ToArray();

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

                                var distance = sphereB.Center - sphereA.Center;
                                distance = vmod(distance + new Vector3(range / 2)) - new Vector3(range / 2);

                                if (distance.LengthSquared() < (sphereA.Radius + sphereB.Radius) * (sphereA.Radius + sphereB.Radius))
                                {
                                    interact(
                                        (objectA, sphereA.Center + distance / 2),
                                        (objectB, sphereB.Center - distance / 2)
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

            for (var z = minpos.Z; z <= maxpos.Z; z++)
            {
                for (var x = minpos.X; x <= maxpos.X; x++)
                {
                    foreach(var c in grid[imod(x), imod(z)].staticColliders.OfType<Collider>())
                    {
                        yield return c;
                    }
                }
            }
        }
    }
}
