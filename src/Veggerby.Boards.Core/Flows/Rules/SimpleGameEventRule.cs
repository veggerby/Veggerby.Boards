using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class SimpleGameEventRule<T> : GameEventRule<T> where T : IGameEvent
    {
        private readonly Func<GameState, T, RuleCheckState> _handler;

        private SimpleGameEventRule(Func<GameState, T, RuleCheckState> handler, IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
            : base(onBeforeEvent, onAfterEvent)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handler = handler;
        }

        public override RuleCheckState Check(GameState gameState, T @event)
        {
            return _handler(gameState, @event);
        }

        public static IGameEventRule<T> New(Func<RuleCheckState> handler = null, IStateMutator<T> onBeforeEvent = null, IStateMutator<T> onAfterEvent = null)
        {
            return new SimpleGameEventRule<T>((state, @event) => handler != null ? handler() : RuleCheckState.Valid, onBeforeEvent, onAfterEvent ?? new NullStateMutator<T>());
        }
    }
}