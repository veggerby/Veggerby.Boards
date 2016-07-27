using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Phases
{
    public class Turn
    {
        public Player Player { get; }

        public Round Round { get; }

        public Turn(Round round, Player player)
        {
            if (round == null)
            {
                throw new ArgumentNullException(nameof(round));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            Round = round;
            Player = player;
        }

        protected bool Equals(Turn other)
        {
            return Player.Equals(other.Player) && Round.Equals(other.Round);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Turn)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Player?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Round?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}