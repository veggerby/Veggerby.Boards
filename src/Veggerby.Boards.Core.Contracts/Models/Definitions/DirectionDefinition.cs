namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class DirectionDefinition
    {
        public DirectionDefinition(string directionId)
        {
            DirectionId = directionId;
        }

        public string DirectionId { get; }
    }
}