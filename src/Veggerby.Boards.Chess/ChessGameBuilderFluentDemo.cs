using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Chess.Conditions;
using Veggerby.Boards.Chess.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Demonstration of the new fluent API for ChessGameBuilder.
/// This partial class shows how chess rules can be refactored using the improved
/// lambda-based API with better visual hierarchy and maintainability.
/// </summary>
/// <remarks>
/// This is a proof-of-concept refactoring showing the benefits of the new API:
/// 1. Clear visual scoping via lambda blocks
/// 2. Reduced duplication through ConditionGroup helpers
/// 3. Easier extraction of rule groups into named helper methods
/// 4. Better IntelliSense experience with smaller context scopes
/// </remarks>
public partial class ChessGameBuilderFluentDemo : GameBuilder
{
    /// <summary>
    /// Demonstrates the new DefineRules API for chess movement phase.
    /// Compare this to the traditional Then() API in ChessGameBuilder.cs.
    /// </summary>
    protected void BuildMovePhaseFluent()
    {
        AddGamePhase("move pieces")
            .WithEndGameDetection(
                game => new CheckmateOrStalemateCondition(game),
                game => new ChessEndGameMutator(game))
            .If<GameNotEndedCondition>()
            .DefineRules(phase =>
            {
                // Castling rules (special king move)
                DefineCastlingRules(phase);

                // Non-pawn movement rules
                DefineNonPawnCaptureRules(phase);
                DefineNonPawnNormalMoveRules(phase);

                // Pawn-specific movement rules
                DefinePawnCaptureRules(phase);
                DefinePawnEnPassantRules(phase);
                DefinePawnDoubleStepRules(phase);
                DefinePawnNormalMoveRules(phase);
            });
    }

    /// <summary>
    /// Defines castling rules (must appear before generic king movement).
    /// </summary>
    private void DefineCastlingRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .And<CastlingGameEventCondition>()
            .And<CastlingKingSafetyGameEventCondition>()
            .Execute(m => m
                .Apply<CastlingMoveMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines generic non-pawn capture rules.
    /// </summary>
    private void DefineNonPawnCaptureRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .With(ChessConditions.NonPawn)
            .With(ChessConditions.UnobstructedToOpponentPiece)
            .Execute(m => m
                .Apply<ChessCapturePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines generic non-pawn normal move rules.
    /// </summary>
    private void DefineNonPawnNormalMoveRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .With(ChessConditions.NonPawn)
            .With(ChessConditions.UnobstructedToEmpty)
            .Execute(m => m
                .Apply<ChessMovePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines pawn diagonal capture rules.
    /// </summary>
    private void DefinePawnCaptureRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .With(ChessConditions.PawnDiagonalCapture)
            .And<PathNotObstructedGameEventCondition>()
            .And<DestinationHasOpponentPieceGameEventCondition>()
            .Execute(m => m
                .Apply<ChessCapturePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines pawn en-passant capture rules.
    /// </summary>
    private void DefinePawnEnPassantRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .With(ChessConditions.PawnDiagonalCapture)
            .And<EnPassantCaptureGameEventCondition>()
            .Execute(m => m
                .Apply<EnPassantCapturePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines pawn double-step advance rules.
    /// </summary>
    private void DefinePawnDoubleStepRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .And<PathNotObstructedGameEventCondition>()
            .And<DestinationIsEmptyGameEventCondition>()
            .And<DistanceTwoGameEventCondition>()
            .And<PawnInitialDoubleStepGameEventCondition>()
            .Execute(m => m
                .Apply<ChessMovePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Defines pawn normal single-step forward move rules.
    /// </summary>
    private void DefinePawnNormalMoveRules(IPhaseRuleBuilder phase)
    {
        phase.On<MovePieceGameEvent>(evt => evt
            .With(ChessConditions.ActivePlayerMove)
            .With(ChessConditions.PawnNormalMove)
            .And<PathNotObstructedGameEventCondition>()
            .And<DestinationIsEmptyGameEventCondition>()
            .Execute(m => m
                .Apply<ChessMovePieceStateMutator>()
                .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
    }

    /// <summary>
    /// Placeholder Build() to satisfy GameBuilder contract.
    /// In a real refactoring, this would call BuildMovePhaseFluent() after board/piece setup.
    /// </summary>
    protected override void Build()
    {
        BoardId = "chess";
        // ... board setup would go here ...
        // BuildMovePhaseFluent();
    }
}
