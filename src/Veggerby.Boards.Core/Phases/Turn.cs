using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Phases
{
    public class Turn
    {
        public Player Player { get; }
        
        public Turn(Player player)
        {
            Player = player;
        }

        protected bool Equals(Turn other)
        {
            return Player?.Equals(other?.Player) ?? false;
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
            return Player.GetHashCode();
        }
    }
}