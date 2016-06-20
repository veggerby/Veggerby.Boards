using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class PieceDefinition
    {
        private readonly IEnumerable<DirectionPatternDefinition> _directionPatterns;

        public PieceDefinition(string pieceId, IEnumerable<DirectionPatternDefinition> directionPatterns)
        {
            PieceId = pieceId;
            _directionPatterns = (directionPatterns ?? Enumerable.Empty<DirectionPatternDefinition>()).ToList();
        }

        public string PieceId { get; }

        public IEnumerable<DirectionPatternDefinition> DirectionPatterns => _directionPatterns;
    }
}
