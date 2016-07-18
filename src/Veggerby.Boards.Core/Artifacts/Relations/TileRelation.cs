namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class TileRelation : ArtifactRelation<Tile, Tile>
    {
        public Direction Direction { get; }

        public int Distance { get; }

        public TileRelation(Tile from, Tile to, Direction direction, int distance = 1) : base(from, to)
        {
            Direction = direction;
            Distance = distance;
        }


        public override string ToString()
        {
            return $"{GetType().Name} {From} --> {To}: {Direction}, {Distance}";
        }

    }
}