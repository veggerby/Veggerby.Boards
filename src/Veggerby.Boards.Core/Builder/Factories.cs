using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder
{
    public delegate IGameStateCondition GameStateConditionFactory(Game game);
    public delegate IGameEventCondition<T> GameEventConditionFactory<T>(Game game) where T : IGameEvent;
    public delegate IStateMutator<T> StateMutatorFactory<T>(Game game) where T : IGameEvent;
    public delegate IGameEventPreProcessor GameEventPreProcessorFactory(Game game);
}