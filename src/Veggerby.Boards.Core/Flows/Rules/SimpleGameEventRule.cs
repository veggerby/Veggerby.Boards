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
            _handler = handler;
        }

        protected override RuleCheckState Check(GameState gameState, T @event)
        {
            return _handler(gameState, @event);
        }

        public static IGameEventRule New(Func<RuleCheckState> handler, IStateMutator<T> onBeforeEvent = null, IStateMutator<T> onAfterEvent = null)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return new SimpleGameEventRule<T>((state, @event) => handler(), onBeforeEvent, onAfterEvent ?? new NullStateMutator<T>());
        }
    }
}