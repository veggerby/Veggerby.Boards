using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class ActivePlayerState : ArtifactState<Player>
    {
        public bool IsActive { get; }

        public ActivePlayerState(Player player, bool isActive) : base(player)
        {
            IsActive = isActive;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ActivePlayerState);
        }

        public override bool Equals(IArtifactState other)
        {
            return Equals(other as ActivePlayerState);
        }

        public bool Equals(ActivePlayerState other)
        {
            if (other == null)
            {
                return false;
            }

            return Artifact.Equals(other.Artifact) && IsActive.Equals(other.IsActive);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.GetType().GetHashCode();
                hashCode = (hashCode*397) ^ Artifact.GetHashCode();
                hashCode = (hashCode*397) ^ IsActive.GetHashCode();
                return hashCode;
            }
        }
    }
}