namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class DirectionPatternDefinition
    {
        private readonly bool _isRepeatable;
        private readonly DirectionDefinition[] _directions;

        public DirectionPatternDefinition(bool isRepeatable, params DirectionDefinition[] directions)
        {
            _isRepeatable = isRepeatable;
            _directions = directions;
        }

        public DirectionDefinition[] Directions => _directions;

        public bool IsRepeatable => _isRepeatable;
    }
}
