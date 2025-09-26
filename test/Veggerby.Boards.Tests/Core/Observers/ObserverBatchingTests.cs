using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using Xunit;

namespace Veggerby.Boards.Tests.Core.Observers;

public class ObserverBatchingTests
{
    private sealed class RecordingObserver : IEvaluationObserver
    {
        public readonly System.Collections.Generic.List<string> Entries = new();
        public void OnPhaseEnter(GamePhase phase, GameState state) => Entries.Add($"phase:{phase.Number}");
        public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex) => Entries.Add($"eval:{ruleIndex}:{response.Result}");
        public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex) => Entries.Add($"applied:{ruleIndex}");
        public void OnEventIgnored(IGameEvent @event, GameState state) => Entries.Add("ignored");
        public void OnStateHashed(GameState state, ulong hash) => Entries.Add($"hash:{hash}");
        public void OnRuleSkipped(GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex) => Entries.Add($"skip:{ruleIndex}:{reason}");
    }

    [Fact]
    public void GivenSingleMove_WhenBatchedEnabled_ThenOrderingMatchesUnbatched()
    {
        // arrange
        // DecisionPlan always enabled (legacy path removed)
        Boards.Internal.FeatureFlags.EnableObserverBatching = false;
        var rec1 = new RecordingObserver();
        var unbatched = new ChessGameBuilder().WithObserver(rec1).Compile();

        var piece = unbatched.Game.GetPiece("white-pawn-5"); // e2 pawn
        var path = TestPathHelper.ResolveFirstValidPath(unbatched.Game, piece, "e2", "e4", "e3");
        Assert.NotNull(path); // if this fails movement semantics changed.
        var evt = new MovePieceGameEvent(piece, path);

        unbatched = unbatched.HandleEvent(evt);
        var sequenceUnbatched = rec1.Entries.ToArray();

        Boards.Internal.FeatureFlags.EnableObserverBatching = true;
        var rec2 = new RecordingObserver();
        var batched = new ChessGameBuilder().WithObserver(rec2).Compile();
        // Re-resolve path for new game instance (cannot reuse path object bound to previous board)
        var piece2 = batched.Game.GetPiece("white-pawn-5");
        var path2 = TestPathHelper.ResolveFirstValidPath(batched.Game, piece2, "e2", "e4", "e3");
        Assert.NotNull(path2);
        var evt2 = new MovePieceGameEvent(piece2, path2);
        batched = batched.HandleEvent(evt2);
        var sequenceBatched = rec2.Entries.ToArray();

        // assert
        Assert.Equal(sequenceUnbatched.Length, sequenceBatched.Length);
        for (var i = 0; i < sequenceUnbatched.Length; i++)
        {
            Assert.Equal(sequenceUnbatched[i], sequenceBatched[i]);
        }
    }
}