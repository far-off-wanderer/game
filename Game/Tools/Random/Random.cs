namespace Far_Off_Wanderer
{
    public class Random
    {
        public Random()
        {
            var random = new System.Random();
            Real = new(random);
            Integer = new(random);
            Vector2D = new(Real);
            Vector3D = new(Real);
        }

        public RandomHelpers.Real Real { get; }
        public RandomHelpers.Integer Integer { get; }
        public RandomHelpers.Vector2D Vector2D { get; }
        public RandomHelpers.Vector3D Vector3D { get; }
    }
}