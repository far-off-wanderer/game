using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Far_Off_Wanderer
{
    public class InfiniteTerrainDistanceField
    {
        Vector3 position;
        Vector3 size;

        float[,,] cells;

        Vector3 MinCorner => position - size / 2;
        Vector3 MaxCorner => position + size / 2;

        Vector3 Vmod(Vector3 v) => new Vector3(
            (float)(v.X - size.X * Math.Floor(v.X / size.X)),
            (float)(v.Y - size.Y * Math.Floor(v.Y / size.Y)),
            (float)(v.Z - size.Z * Math.Floor(v.Z / size.Z))
        );

        Vector3 Vmod2(Vector3 v) => Vmod(v + size / 2) - size / 2;

        public InfiniteTerrainDistanceField(Terrain terrain)
        {
            this.position = terrain.Position;
            this.size = terrain.Size;
            this.cells = new float[terrain.DataWidth, 16, terrain.DataWidth];
            this.cells.Set(float.MaxValue);

            if (LoadFromCache() == false)
            {

                var grid = new Grid(size.X);
                grid.AddStaticColliders(terrain.Colliders);

                this.cells.SetInParallel((value, x, y, z) =>
                {
                    var p = new Vector3[8];
                    for (var dz = 0; dz <= 1; dz++)
                    {
                        for (var dy = 0; dy <= 1; dy++)
                        {
                            for (var dx = 0; dx <= 1; dx++)
                            {
                                p[dx + 2 * dy + 4 * dz] = new Vector3(
                                    MathHelper.Lerp(MinCorner.X, MaxCorner.X, 1f * (x + dx) / cells.LengthX()),
                                    MathHelper.Lerp(MinCorner.Y, MaxCorner.Y, 1f * (y + dy) / cells.LengthY()),
                                    MathHelper.Lerp(MinCorner.Z, MaxCorner.Z, 1f * (z + dz) / cells.LengthZ())
                                );
                            }
                        }
                    }

                    var radius = 10f;
                    var loop = 0;
                    var list = default(IEnumerable<Collider>);
                    do
                    {
                        list = grid.GetNearby(p[0], radius);
                        radius *= 1.4f;
                        loop++;
                    } while (list.Any() == false);

                    var distance = list.Min(c =>
                    {
                        return p.Min(p_ => Vmod2(c.Position - p_).Length() - c.Radius);
                    });

                    list = grid.GetNearby(p[0], distance);

                    var finalDistance = distance;
                    if (list.Any())
                    {
                        finalDistance = list.Min(c =>
                        {
                            return p.Min(p_ => Vmod2(c.Position - p_).Length() - c.Radius);
                        });
                    }

                    return finalDistance;
                });

                SaveToCache();
            }
        }

        public static bool HasCache => File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, "distancefield.cache"));

        private void SaveToCache()
        {
            var cachePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "distancefield.cache");

            var cache = new List<float>();

            cells.Get(value => cache.Add(value));

            using (var writer = new BinaryWriter(File.OpenWrite(cachePath)))
            {
                foreach (var value in cache)
                {
                    writer.Write(value);
                }
            }
        }

        private bool LoadFromCache()
        {
            //var cachePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "distancefield.cache");
            //if(File.Exists(cachePath) == false)
            //{
            //    return false;
            //}

            var cachePath = Path.Combine(Package.Current.InstalledLocation.Path, "Content", "distancefield.cache");

            var cache = new Queue<float>();

            using (var reader = new BinaryReader(File.OpenRead(cachePath)))
            {
                foreach(var _ in Enumerable.Range(0, cells.LengthX() * cells.LengthY() * cells.LengthZ()))
                {
                    cache.Enqueue(reader.ReadSingle());
                }
            }
            cells.Set(value => cache.Dequeue());
            return true;
        }

        (int x, int y, int z) Floor(Vector3 v) => ((int)Math.Floor(v.X), (int)Math.Floor(v.Y), (int)Math.Floor(v.Z));

        public float DistanceAt(Vector3 position)
        {
            var p = Vmod(position - MinCorner) / size;

            var (x, y, z) = Floor(p * cells.Lengths());

            return cells[x, y, z];
        }
    }
}
