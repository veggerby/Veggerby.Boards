using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class NullDiceState : ArtifactState<Dice>
    {
        public NullDiceState(Dice dice) : base(dice)
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NullDiceState);
        }

        public override bool Equals(IArtifactState other)
        {
            return Equals(other as NullDiceState);
        }

        public bool Equals(NullDiceState other)
        {
            if (other == null)
            {
                return false;
            }

            return Artifact.Equals(other.Artifact);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.GetType().GetHashCode();
                hashCode = (hashCode*397) ^ Artifact.GetHashCode();
                return hashCode;
            }
        }
    }
}