namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class TileRelation : ArtifactRelation<Tile, Tile>
    {
        public Direction Direction { get; }

        public int Distance { get; }

        public TileRelation(Tile source, Tile destination, Direction direction, int distance = 1) : base(source, destination)
        {
            Direction = direction;
            Distance = distance;
        }
    }
}