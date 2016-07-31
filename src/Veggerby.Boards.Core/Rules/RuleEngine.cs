using System.Collections.Generic;

namespace Veggerby.Boards.Core.Rules
{
    public class RuleEngine : CompositionRule
    {
        public RuleEngine(IEnumerable<IRule> rules) : base(rules, true)
        {
        }
    }
}