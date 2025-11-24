using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.Conditions;
using Veggerby.Boards.Chess.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;
namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Scenario where a white pawn attempts a double-step but the destination tile is occupied by an opponent piece (should be invalid / ignored).
/// </summary>
internal sealed class DestinationOccupiedDoubleStepScenarioBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "chess-destination-occupied-double-step";

        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);

        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, true);
        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black, false);

        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.West);
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.NorthWest);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.SouthWest);

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

        AddPiece("white-pawn-test")
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
            .HasDirection(Constants.Directions.North).Done()
            .HasPattern(Constants.Directions.North)
            .HasPattern(Constants.Directions.North, Constants.Directions.North)
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done();

        // Opponent piece placed on e4 (double-step destination from e2)
        AddPiece("black-bishop-blocker")
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
            .HasDirection(Constants.Directions.North).Done();

        WithPiece("white-pawn-test").OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E2);
        WithPiece("black-bishop-blocker").OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E4);

        WithState(new ChessStateExtras(true, true, true, true, null, 0, 1, Array.Empty<string>()));

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