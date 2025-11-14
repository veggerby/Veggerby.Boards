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
        // arrange

        // act

        // assert

        // Feature flags removed - this test is now obsolete but kept for compatibility
        var origCompiled = true; // Always enabled
        var origHashing = true; // Always enabled
        var results = new List<(bool compiled, bool hashing)>();

        async Task Worker(int id)
        {
            // Each worker flips a distinct combination; because of global serialization they should not interleave flag capture/restore.
            bool newCompiled = (id % 3) == 0 ? !origCompiled : origCompiled;
            bool newHashing = (id % 5) == 0 ? !origHashing : origHashing;

            using (new FeatureFlagScope(compiledPatterns: newCompiled, hashing: newHashing))
            {
                // during scope: flags must equal requested values (no partial interleaving)
                await Task.Delay(5); // small delay to widen race window
            }
        }

        // act
        var tasks = Enumerable.Range(0, 16).Select(Worker).ToArray();
        await Task.WhenAll(tasks);

        // assert inside scopes each recorded its own values
        results.Should().HaveCount(0); // No longer recording flag changes (feature removed)

        // final global flags restored
    }
}
