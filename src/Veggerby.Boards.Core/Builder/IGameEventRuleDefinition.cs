using Veggerby.Boards.Core.Flows.Rules;

namespace Veggerby.Boards.Core.Builder
{
    public interface IGameEventRuleDefinition
    {
        IGameEventRule Build();
    }
}