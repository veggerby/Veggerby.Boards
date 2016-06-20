using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class DirectionPatternDefinition
    {
        private readonly IEnumerable<DirectionDefinition> _directions;

        public DirectionPatternDefinition(bool isRepeatable, IEnumerable<DirectionDefinition> directions)
        {
            IsRepeatable = isRepeatable;
            _directions = (directions ?? Enumerable.Empty<DirectionDefinition>()).ToList();;
        }

        public IEnumerable<DirectionDefinition> Directions => _directions;

        public bool IsRepeatable { get; }
    }
}
