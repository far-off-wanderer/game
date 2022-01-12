namespace Far_Off_Wanderer.RandomHelpers;

public class Integer
{
    private System.Random random;

    public Integer(System.Random random)
    {
        this.random = random;
    }

    public int UpTo(int max) => random.Next(max);
    public int Between(int min, int max) => random.Next(min, max);
}