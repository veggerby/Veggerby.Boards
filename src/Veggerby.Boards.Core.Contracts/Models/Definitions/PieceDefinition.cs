using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class PieceDefinition
    {
        private readonly string _pieceId;
        private readonly IEnumerable<DirectionPatternDefinition> _directionPatterns;

        public PieceDefinition(string pieceId, IEnumerable<DirectionPatternDefinition> directionPatterns)
        {
            _pieceId = pieceId;
            _directionPatterns = (directionPatterns ?? Enumerable.Empty<DirectionPatternDefinition>()).ToList();
        }

        public string PieceId => _pieceId;

        public IEnumerable<DirectionPatternDefinition> DirectionPatterns => _directionPatterns;
    }
}
