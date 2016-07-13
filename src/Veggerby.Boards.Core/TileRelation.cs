namespace Veggerby.Boards.Core
{
    public class TileRelation : ArtifactRelation<Tile, Tile>
    {
        public TileRelationDirection Direction { get; }

        public TileRelation(Tile source, Tile destination, TileRelationDirection direction) : base(source, destination)
        {
            Direction = direction;
        }
    }
}