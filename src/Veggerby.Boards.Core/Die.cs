namespace Veggerby.Boards.Core
{
    public abstract class Die<T> : Artifact
    {
        public T MinValue { get; }
        public T MaxValue { get; }
        public abstract T Roll();

        public Die(string id, T minValue = default(T), T maxValue = default(T)) : base(id)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}