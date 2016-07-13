using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class RuleEngine
    {
        private readonly IEnumerable<IRule> _rules;

        public RuleEngine(IEnumerable<IRule> rules)
        {
            _rules = (rules ?? Enumerable.Empty<IRule>()).ToList();
        }

        public GameState GetState(GameState currentState, IGameEvent @event)
        {
            var newStates = _rules
                .Select(x => x.GetState(currentState, @event))
                .Where(x => x != null)
                .ToList();

            if (!newStates.Any())
            {
                return null;
            }

            if (newStates.Count() == 1)
            {
                return newStates.Single();
            }

            throw new BoardException("Event does not result in unambiguous state");
        }
    }
}