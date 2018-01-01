using Microsoft.Xna.Framework;

namespace Conesoft.Engine.Random
{
    interface IRandom
    {
        float NextFloat();
        Vector3 NextVector3();
        Vector3 NextDirection();
        Vector3 NextPointInUnitSphere();
    }

    namespace Implementation
    {
        class Random : IRandom
        {
            private System.Random r = new System.Random();

            public float NextFloat()
            {
                return (float)r.NextDouble();
            }

            public Vector3 NextVector3()
            {
                return new Vector3(NextFloat(), NextFloat(), NextFloat());
            }

            public Vector3 NextDirection()
            {
                return Vector3.Normalize(NextPointInUnitSphere());
            }

            public Vector3 NextPointInUnitSphere()
            {
                var direction = Vector3.Zero;
                do
                {
                    direction = NextVector3();
                } while (direction.LengthSquared() > 1);
                return direction;
            }
        }
    }
}
