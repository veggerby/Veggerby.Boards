using System.Collections.Generic;

namespace Veggerby.Boards.Core.Rules
{
    public class RuleEngine : CompositeRule
    {
        public RuleEngine(IEnumerable<IRule> rules) : base(rules, true)
        {
        }
    }
}