using System.Collections.Generic;

namespace Veggerby.Boards.Core.States
{
    public class StateBaseEntityEqualityComparer : IEqualityComparer<IState>
    {
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

            return (x as IArtifactState)?.Artifact.Equals((y as IArtifactState)?.Artifact) ?? false;
        }

        public int GetHashCode(IState obj)
        {
            return (obj as IArtifactState)?.Artifact.GetHashCode() ?? obj.GetHashCode();
        }
    }
}