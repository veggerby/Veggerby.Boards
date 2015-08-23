namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class DirectionDefinition
    {
        private readonly string _directionId;

        public DirectionDefinition(string directionId)
        {
            _directionId = directionId;
        }

        public string DirectionId => _directionId;
    }
}