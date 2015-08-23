using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Core.Contracts.Models
{
    public class DirectionPatternDefinition
    {
        private readonly DirectionDefinition[] _directions;

        public DirectionPatternDefinition(params DirectionDefinition[] directions)
        {
            _directions = directions;
        }

        public DirectionDefinition[] Directions => _directions;
    }
}
