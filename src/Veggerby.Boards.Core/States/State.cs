namespace Veggerby.Boards.Core.States
{
    public abstract class State<T> : IState
        where T : Artifact
    {
        public T Artifact { get; }

        public State(T artifact)
        {
            Artifact = artifact;
        }
    }
}