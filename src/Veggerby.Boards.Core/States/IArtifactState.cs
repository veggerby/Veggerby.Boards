using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public interface IArtifactState : IState
    {
        Artifact Artifact { get; }
    }
}