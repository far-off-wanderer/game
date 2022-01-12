using Microsoft.Xna.Framework;

namespace Far_Off_Wanderer.RandomHelpers;

public class Vector3D
{
    private Real real;

    public Vector3D(Real real)
    {
        this.real = real;
    }

    public Vector3 Unsigned => new(real.Unsigned, real.Unsigned, real.Unsigned);

    public Vector3 Signed => new(real.Signed, real.Signed, real.Signed);

    public Vector3 Direction => Vector3.Normalize(UnitPoint);

    public Vector3 UnitPoint
    {
        get
        {
            var direction = Vector3.Zero;
            do
            {
                direction.X = real.Signed;
                direction.Y = real.Signed;
                direction.Z = real.Signed;
            } while (direction.LengthSquared() > 1);
            return direction;
        }
    }
}
