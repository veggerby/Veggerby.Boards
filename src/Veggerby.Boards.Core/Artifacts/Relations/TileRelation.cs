namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class TileRelation : ArtifactRelation<Tile, Tile>
    {
        public Direction Direction { get; }

        public TileRelation(Tile source, Tile destination, Direction direction) : base(source, destination)
        {
            Direction = direction;
        }
    }
}