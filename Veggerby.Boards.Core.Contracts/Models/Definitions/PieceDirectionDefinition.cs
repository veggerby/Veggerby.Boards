namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class PieceDirectionDefinition
    {
        private readonly PieceDefinition _piece;
        private readonly DirectionPatternDefinition _directionPattern;

        public PieceDirectionDefinition(PieceDefinition piece, DirectionPatternDefinition directionPattern)
        {
            _piece = piece;
            _directionPattern = directionPattern;
        }

        public PieceDefinition Piece => _piece;

        public DirectionPatternDefinition DirectionPattern => _directionPattern;
    }
}