namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class Direction
    {
        public static Direction Left = new Direction("left");
        public static Direction Right = new Direction("right");

        public static Direction North = new Direction("north");
        public static Direction South = new Direction("south");
        public static Direction East = new Direction("east");
        public static Direction West = new Direction("west");

        public static Direction NorthWest = new Direction("north-west");
        public static Direction NorthEast = new Direction("north-east");
        public static Direction SouthWest = new Direction("south-west");
        public static Direction SouthEast = new Direction("south-east");

        public static Direction Clockwise = new Direction("clockwise");
        public static Direction CounterClockwise = new Direction("counter-clockwise");
        
        public string Id { get; }

        public Direction(string id)
        {
            Id = id;
        }

        protected bool Equals(Direction other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Direction)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}