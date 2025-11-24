using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.Conditions;
using Veggerby.Boards.Chess.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;
namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Scenario where a white pawn attempts a double-step but the intermediate tile is occupied by another (blocking) piece.
/// Board topology is full 8x8 to reuse path logic; only minimal pieces are added.
/// </summary>
internal sealed class BlockedDoubleStepScenarioBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "chess-blocked-double-step";

        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);

        // Active player: white to move.
        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, true);
        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black, false);

        // Directions
        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.West);
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.NorthWest);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.SouthWest);

        // Tiles (standard orientation: rank increasing = north)
        static string FileChar(int idx) => ((char)('a' + idx - 1)).ToString();
        for (var file = 1; file <= 8; file++)
        {
            for (var rank = 1; rank <= 8; rank++)
            {
                var id = $"tile-{FileChar(file)}{rank}";
                var tile = AddTile(id);
                if (file < 8)
                {
                    tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank}").InDirection(Constants.Directions.East);
                }
                if (file > 1)
                {
                    tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank}").InDirection(Constants.Directions.West);
                }
                if (rank < 8)
                {
                    tile.WithRelationTo($"tile-{FileChar(file)}{rank + 1}").InDirection(Constants.Directions.North);
                }
                if (rank > 1)
                {
                    tile.WithRelationTo($"tile-{FileChar(file)}{rank - 1}").InDirection(Constants.Directions.South);
                }
                if (file < 8 && rank < 8)
                {
                    tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank + 1}").InDirection(Constants.Directions.NorthEast);
                }
                if (file > 1 && rank < 8)
                {
                    tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank + 1}").InDirection(Constants.Directions.NorthWest);
                }
                if (file < 8 && rank > 1)
                {
                    tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank - 1}").InDirection(Constants.Directions.SouthEast);
                }
                if (file > 1 && rank > 1)
                {
                    tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank - 1}").InDirection(Constants.Directions.SouthWest);
                }
            }
        }

        // White test pawn with standard patterns
        AddPiece("white-pawn-test")
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
            .HasDirection(Constants.Directions.North).Done()
            .HasPattern(Constants.Directions.North)
            .HasPattern(Constants.Directions.North, Constants.Directions.North)
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done();

        // Blocking piece (use a black knight to ensure NonPawnGameEventCondition for any incidental filtering) placed on intermediate square e3
        AddPiece("black-knight-blocker")
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
            .HasDirection(Constants.Directions.North).Done(); // minimal direction to satisfy construction

        WithPiece("white-pawn-test").OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E2);
        WithPiece("black-knight-blocker").OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E3);

        WithState(new ChessStateExtras(true, true, true, true, null, 0, 1, Array.Empty<string>()));

        // Movement phase replicate ordering: only need forward move & double-step (capture rules unnecessary here)
        AddGamePhase("move pieces")
            .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                            .And<DistanceTwoGameEventCondition>()
                            .And<PawnInitialDoubleStepGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(g => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<ForwardPawnDirectionGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(g => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }
}