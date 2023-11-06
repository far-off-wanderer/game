namespace Far_Off_Wanderer.RandomHelpers;

public class Real
{
    private System.Random random;

    public Real(System.Random random)
    {
        this.random = random;
    }

    public float UpTo(float max) => Unsigned * max;
    public float Between(float min, float max) => Unsigned * (max - min) + min;

    public float Unsigned => random.NextSingle();

    public float Signed => Unsigned * 2f - 1f;

    public float Direction => Signed >= 0f ? 1f : -1f;
}