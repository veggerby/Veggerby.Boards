using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Minimal chess scenario builder for focused en-passant tests (black captures white double-step).
/// Provides full 8x8 board topology but only includes required pawns (optionally an auxiliary pawn for non-capture reply tests).
/// </summary>
internal sealed class EnPassantScenarioBuilder : GameBuilder
{
    private readonly bool _includeAuxiliaryBlackPawn;

    public EnPassantScenarioBuilder(bool includeAuxiliaryBlackPawn = false)
    {
        _includeAuxiliaryBlackPawn = includeAuxiliaryBlackPawn;
    }

    protected override void Build()
    {
        BoardId = "chess-en-passant-scenario";

        AddPlayer(ChessIds.Players.White);
        AddPlayer(ChessIds.Players.Black);

        // Active player projections: white starts, black inactive.
        WithActivePlayer(ChessIds.Players.White, true);
        WithActivePlayer(ChessIds.Players.Black, false);

        // Reuse canonical directions
        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.West);
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.NorthWest);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.SouthWest);

        // Board tiles + relations (same orientation as ChessGameBuilder after flip: increasing rank = north)
        static string FileChar(int idx) => ((char)('a' + idx - 1)).ToString();
        for (var file = 1; file <= 8; file++)
        {
            for (var rank = 1; rank <= 8; rank++)
            {
                var id = $"tile-{FileChar(file)}{rank}";
                var tile = AddTile(id);
                if (file < 8) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank}").InDirection(Constants.Directions.East);
                if (file > 1) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank}").InDirection(Constants.Directions.West);
                if (rank < 8) tile.WithRelationTo($"tile-{FileChar(file)}{rank + 1}").InDirection(Constants.Directions.North);
                if (rank > 1) tile.WithRelationTo($"tile-{FileChar(file)}{rank - 1}").InDirection(Constants.Directions.South);
                if (file < 8 && rank < 8) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank + 1}").InDirection(Constants.Directions.NorthEast);
                if (file > 1 && rank < 8) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank + 1}").InDirection(Constants.Directions.NorthWest);
                if (file < 8 && rank > 1) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank - 1}").InDirection(Constants.Directions.SouthEast);
                if (file > 1 && rank > 1) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank - 1}").InDirection(Constants.Directions.SouthWest);
            }
        }

        // Structural pawn patterns (white north, black south)
        AddPiece("white-pawn-test")
            .WithOwner(ChessIds.Players.White)
            .HasDirection(Constants.Directions.North).Done()
            .HasPattern(Constants.Directions.North)
            .HasPattern(Constants.Directions.North, Constants.Directions.North)
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done();

        AddPiece("black-pawn-test")
            .WithOwner(ChessIds.Players.Black)
            .HasDirection(Constants.Directions.South).Done()
            .HasPattern(Constants.Directions.South)
            .HasPattern(Constants.Directions.South, Constants.Directions.South)
            .HasDirection(Constants.Directions.SouthEast).Done()
            .HasDirection(Constants.Directions.SouthWest).Done();

        if (_includeAuxiliaryBlackPawn)
        {
            AddPiece("black-pawn-aux")
                .WithOwner(ChessIds.Players.Black)
                .HasDirection(Constants.Directions.South).Done()
                .HasPattern(Constants.Directions.South)
                .HasPattern(Constants.Directions.South, Constants.Directions.South)
                .HasDirection(Constants.Directions.SouthEast).Done()
                .HasDirection(Constants.Directions.SouthWest).Done();
        }

        // Initial placement: white pawn on e2, black pawn on d4 (ready to capture e3 after double-step)
        WithPiece("white-pawn-test").OnTile(ChessIds.Tiles.E2);
        WithPiece("black-pawn-test").OnTile(ChessIds.Tiles.D4);
        if (_includeAuxiliaryBlackPawn)
        {
            WithPiece("black-pawn-aux").OnTile(ChessIds.Tiles.H7);
        }

        // Chess extras baseline
        WithState(new ChessStateExtras(true, true, true, true, null, 0, 1, System.Array.Empty<string>()));

        // Role metadata for test pieces
        var roles = new System.Collections.Generic.Dictionary<string, ChessPieceRole>
        {
            { "white-pawn-test", ChessPieceRole.Pawn },
            { "black-pawn-test", ChessPieceRole.Pawn }
        };

        if (_includeAuxiliaryBlackPawn)
        {
            roles.Add("black-pawn-aux", ChessPieceRole.Pawn);
        }

        WithState(new ChessPieceRolesExtras(roles));

        // Color metadata for test pieces
        var colors = new System.Collections.Generic.Dictionary<string, ChessPieceColor>
        {
            { "white-pawn-test", ChessPieceColor.White },
            { "black-pawn-test", ChessPieceColor.Black }
        };
        if (_includeAuxiliaryBlackPawn)
        {
            colors.Add("black-pawn-aux", ChessPieceColor.Black);
        }
        WithState(new ChessPieceColorsExtras(colors));

        // Guard: ensure every added piece has both role and color metadata (defensive for future edits)
        Tests.Chess.Support.MetadataCoverageGuard.AssertAllPiecesCovered(this, roles, colors);

        // Active player projection now explicit via WithActivePlayer declarations above.

        // Movement phase (mirrors ChessGameBuilder ordering)
        AddGamePhase("move pieces")
            .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<EnPassantCaptureGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                    .Then()
                        .Do<EnPassantCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                            .And<DistanceTwoGameEventCondition>()
                            .And<PawnInitialDoubleStepGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<ForwardPawnDirectionGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }
}