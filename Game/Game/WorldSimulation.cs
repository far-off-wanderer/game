namespace Far_Off_Wanderer.Game;

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class WorldSimulation
{
    List<Object3D> objects3D = new();

    public IEnumerable<Object3D> Objects3D => objects3D;

    public void Add(IEnumerable<Object3D> objects) => objects3D.AddRange(objects);
    public void Add(Object3D obj) => objects3D.Add(obj);

    public IEnumerable<T> GetAll<T>() where T : Object3D => objects3D.OfType<T>();

    public bool IsEmpty => objects3D.Count == 0;

    public void Update(TimeSpan elapsedTime, Environment environment)
    {
        var newObjects = new List<Object3D>();
        foreach (var objects3d in objects3D)
        {
            newObjects.AddRange(objects3d.Update(environment, elapsedTime));
        }

        var collidableObjects = objects3D.Where(o => o.Radius > 0);

        var landscapes = collidableObjects.OfType<Landscape>().ToArray();

        var sphericalObjects = collidableObjects.Except(landscapes).ToArray();

        for (var i1 = 0; i1 < sphericalObjects.Length; i1++)
        {
            var obj1 = sphericalObjects[i1];
            for (var i2 = i1 + 1; i2 < sphericalObjects.Length; i2++)
            {
                var obj2 = sphericalObjects[i2];
                if ((obj1.Position - obj2.Position).LengthSquared() <= (obj1.Radius + obj2.Radius) * (obj1.Radius + obj2.Radius))
                {
                    var point = obj1.Position + Vector3.Normalize(obj1.Position - obj2.Position) * obj1.Radius;
                    newObjects.AddRange(obj1.Die(environment, point));
                    newObjects.AddRange(obj2.Die(environment, point));
                }
            }
        }

        foreach (var landscape in landscapes)
        {
            foreach (var obj in sphericalObjects)
            {
                var distanceTo = (obj.Position - landscape.Position) / landscape.Radius;
                var radius = obj.Radius / landscape.Radius;

                var points = landscape.GetHeightsAround1D(distanceTo.X, distanceTo.Z, radius);
                if (points != null)
                {
                    var anyPointsAboveObject = points.Any(p => p.Y > distanceTo.Y - radius);
                    var anyPointsBelowObject = points.Any(p => p.Y < distanceTo.Y + radius);

                    if (anyPointsAboveObject && anyPointsBelowObject)
                    {
                        var point = points.Where(p => p.Y > distanceTo.Y - radius).First();
                        newObjects.AddRange(obj.Die(environment, point));
                    }
                }
            }
        }

        foreach (var newObject in newObjects)
        {
            newObject.Update(environment, elapsedTime);
        }

        objects3D.AddRange(newObjects);
        objects3D = objects3D.Where(obj => obj.Alive == true).ToList();
    }
}
