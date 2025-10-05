using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Capture;

/// <summary>
/// Minimal builder (3 linear tiles) to isolate capture rule semantics without full chess setup.
/// Layout: a1 -- a2 -- a3 (direction east). White slider at a1, black target at a3.
/// </summary>
internal sealed class MinimalCaptureGameBuilder : GameBuilder
{
    public MinimalCaptureGameBuilder()
    {
        // attach observer for tests (records applied mutators)
        WithObserver(CaptureTrackingObserver.Instance);
    }

    protected override void Build()
    {
        BoardId = "mini-capture";
        AddPlayer(ChessIds.Players.White);
        AddPlayer(ChessIds.Players.Black);
        AddDirection(Constants.Directions.East);

        // Minimal forward-only chain a1 -> a2 -> a3 (single direction sufficient for path resolution)
        AddTile(ChessIds.Tiles.A1).WithRelationTo(ChessIds.Tiles.A2).InDirection(Constants.Directions.East);
        AddTile(ChessIds.Tiles.A2).WithRelationTo(ChessIds.Tiles.A3).InDirection(Constants.Directions.East);
        AddTile(ChessIds.Tiles.A3);

        AddPiece("white-slider").WithOwner(ChessIds.Players.White).HasDirection(Constants.Directions.East).CanRepeat();
        AddPiece("black-block").WithOwner(ChessIds.Players.Black).HasDirection(Constants.Directions.East);

        WithPiece("white-slider").OnTile(ChessIds.Tiles.A1);
        WithPiece("black-block").OnTile(ChessIds.Tiles.A3);

        AddGamePhase("move")
            .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If<PathNotObstructedGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<CapturePieceStateMutator>()
                    .ForEvent<MovePieceGameEvent>()
                        .If<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<MovePieceStateMutator>();
    }
}

internal sealed class CaptureTrackingObserver : IEvaluationObserver
{
    public static readonly CaptureTrackingObserver Instance = new CaptureTrackingObserver();
    private CaptureTrackingObserver() { }

    public bool CaptureMutatorApplied { get; private set; }
    public bool MoveMutatorApplied { get; private set; }

    public void OnPhaseEnter(GamePhase phase, GameState state) { }
    public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex) { }
    public void OnEventIgnored(IGameEvent @event, GameState state) { }
    public void OnStateHashed(GameState state, ulong hash) { }
    public void OnRuleSkipped(GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex) { }
    public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex)
    {
        // Heuristic: infer mutator by resulting state change (captured piece present) vs simple move.
        var captured = afterState.GetStates<CapturedPieceState>().FirstOrDefault();
        if (captured is not null)
        {
            CaptureMutatorApplied = true;
        }
        else
        {
            MoveMutatorApplied = true;
        }
    }
}