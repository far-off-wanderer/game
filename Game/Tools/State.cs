namespace Far_Off_Wanderer.Tools;

static class State
{
    public struct TrackedState<T>
    {
        private T previousValue;
        private T currentValue;

        public TrackedState(T value)
        {
            previousValue = value;
            currentValue = value;
        }

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
    public static TrackedState<T> Track<T>(T value) => new(value);
}