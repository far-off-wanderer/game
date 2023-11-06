namespace Far_Off_Wanderer
{
    struct State<T>(T value)
    {
        private T previousValue = value;
        private T currentValue = value;

        public void Set(T value)
        {
            previousValue = currentValue;
            currentValue = value;
        }

        public void Reset(T value)
        {
            previousValue = value;
            currentValue = value;
        }

        public T Value => currentValue;

        public bool HasChanged => previousValue.Equals(currentValue) == false;
    }

    static class State
    {
        public static State<T> Track<T>(T value) => new(value);
    }
}
