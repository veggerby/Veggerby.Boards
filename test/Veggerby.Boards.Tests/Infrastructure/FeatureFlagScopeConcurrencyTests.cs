using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Infrastructure;

public class FeatureFlagScopeConcurrencyTests
{
    [Fact]
    public async Task GivenConcurrentScopes_WhenTogglingDifferentValues_ThenFinalFlagsRestored()
    {
        // arrange (capture originals)
        var origDecisionPlan = FeatureFlags.EnableDecisionPlan;
        var origCompiled = FeatureFlags.EnableCompiledPatterns;
        var origHashing = FeatureFlags.EnableStateHashing;

        var results = new List<(bool decisionPlan, bool compiled, bool hashing)>();

        async Task Worker(int id)
        {
            // Each worker flips a distinct combination; because of global serialization they should not interleave flag capture/restore.
            bool newDecision = (id % 2) == 0 ? !origDecisionPlan : origDecisionPlan;
            bool newCompiled = (id % 3) == 0 ? !origCompiled : origCompiled;
            bool newHashing = (id % 5) == 0 ? !origHashing : origHashing;

            using (new Veggerby.Boards.Tests.Infrastructure.FeatureFlagScope(decisionPlan: newDecision, compiledPatterns: newCompiled, hashing: newHashing))
            {
                // during scope: flags must equal requested values (no partial interleaving)
                FeatureFlags.EnableDecisionPlan.Should().Be(newDecision);
                FeatureFlags.EnableCompiledPatterns.Should().Be(newCompiled);
                FeatureFlags.EnableStateHashing.Should().Be(newHashing);
                await Task.Delay(5); // small delay to widen race window
                results.Add((FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableCompiledPatterns, FeatureFlags.EnableStateHashing));
            }
        }

        // act
        var tasks = Enumerable.Range(0, 16).Select(Worker).ToArray();
        await Task.WhenAll(tasks);

        // assert inside scopes each recorded its own values
        results.Should().HaveCount(16);
        results.Should().Contain(r => r.decisionPlan != origDecisionPlan); // at least some toggled
        results.Should().Contain(r => r.compiled != origCompiled);
        results.Should().Contain(r => r.hashing != origHashing);

        // final global flags restored
        FeatureFlags.EnableDecisionPlan.Should().Be(origDecisionPlan);
        FeatureFlags.EnableCompiledPatterns.Should().Be(origCompiled);
        FeatureFlags.EnableStateHashing.Should().Be(origHashing);
    }
}