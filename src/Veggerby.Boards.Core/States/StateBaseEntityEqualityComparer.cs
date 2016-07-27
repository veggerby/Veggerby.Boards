using System.Collections.Generic;

namespace Veggerby.Boards.Core.States
{
    public class StateBaseEntityEqualityComparer : IEqualityComparer<IState>
    {
        private bool? Equals(IArtifactState x, IArtifactState y)
        {
            return x?.Artifact.Equals(y?.Artifact);
        }

        public bool Equals(IState x, IState y)
        {
            if ((x ?? y) == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return Equals(x as IArtifactState, y as IArtifactState)
                ?? false;
        }

        public int GetHashCode(IState obj)
        {
            return (obj as IArtifactState)?.Artifact.GetHashCode() ?? obj.GetHashCode();
        }
    }
}