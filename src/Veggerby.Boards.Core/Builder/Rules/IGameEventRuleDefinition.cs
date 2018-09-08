using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Rules;

namespace Veggerby.Boards.Core.Builder.Rules
{
    public interface IGameEventRuleDefinition
    {
        IGameEventRule Build(Game game);
    }
}