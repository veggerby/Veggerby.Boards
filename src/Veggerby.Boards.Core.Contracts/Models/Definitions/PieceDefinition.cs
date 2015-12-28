namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class PieceDefinition
    {
        private readonly string _pieceId;
        private readonly DirectionPatternDefinition[] _directionPatterns;

        public PieceDefinition(string pieceId, DirectionPatternDefinition[] directionPatterns)
        {
            _pieceId = pieceId;
            _directionPatterns = directionPatterns;
        }

        public string PieceId => _pieceId;

        public DirectionPatternDefinition[] DirectionPatterns => _directionPatterns;
    }
}
