using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class RuleEngine
    {
        private readonly IEnumerable<Rule> _rules;

        public RuleEngine(IEnumerable<Rule> rules)
        {
            _rules = (rules ?? Enumerable.Empty<Rule>()).ToList();
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