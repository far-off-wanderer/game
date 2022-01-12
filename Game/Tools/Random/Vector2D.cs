using Microsoft.Xna.Framework;

namespace Far_Off_Wanderer.RandomHelpers;

public class Vector2D
{
    private Real real;

    public Vector2D(Real real)
    {
        this.real = real;
    }

    public Vector2 Unsigned => new(real.Unsigned, real.Unsigned);

    public Vector2 Signed => new(real.Signed, real.Signed);

    public Vector2 Direction2D => Vector2.Normalize(UnitPoint);

    public Vector2 UnitPoint
    {
        get
        {
            var direction = Vector2.Zero;
            do
            {
                direction.X = real.Signed;
                direction.Y = real.Signed;
            } while (direction.LengthSquared() > 1);
            return direction;
        }
    }
}