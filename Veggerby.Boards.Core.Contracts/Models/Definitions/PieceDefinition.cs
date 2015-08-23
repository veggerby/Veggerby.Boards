namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class PieceDefinition
    {
        private readonly string _pieceId;

        public PieceDefinition(string pieceId)
        {
            _pieceId = pieceId;
        }

        public string PieceId => _pieceId;
    }
}
