using System.Linq;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Artifacts.Relations;
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
    Veggerby.Boards.Internal.FeatureFlags.EnableDecisionPlan = true; // ensure plan path (more callbacks)
    Veggerby.Boards.Internal.FeatureFlags.EnableObserverBatching = false;
        var rec1 = new RecordingObserver();
    var unbatched = new ChessGameBuilder().WithObserver(rec1).Compile();

        var piece = unbatched.Game.GetPiece("white-pawn-2");
        var from = unbatched.Game.GetTile("e2");
        var to = unbatched.Game.GetTile("e4");
    var path = new ResolveTilePathPatternVisitor(unbatched.Game.Board, from, to).ResultPath!;
        var evt = new MovePieceGameEvent(piece, path);

        unbatched = unbatched.HandleEvent(evt);
        var sequenceUnbatched = rec1.Entries.ToArray();

    Veggerby.Boards.Internal.FeatureFlags.EnableObserverBatching = true;
        var rec2 = new RecordingObserver();
    var batched = new ChessGameBuilder().WithObserver(rec2).Compile();
        batched = batched.HandleEvent(evt);
        var sequenceBatched = rec2.Entries.ToArray();

        // assert
        Assert.Equal(sequenceUnbatched.Length, sequenceBatched.Length);
        for (var i = 0; i < sequenceUnbatched.Length; i++)
        {
            Assert.Equal(sequenceUnbatched[i], sequenceBatched[i]);
        }
    }
}
