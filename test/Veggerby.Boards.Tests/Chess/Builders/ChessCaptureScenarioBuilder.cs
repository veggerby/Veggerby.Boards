using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Chess.Builders;

/// <summary>
/// Minimal chess subset builder: constructs only the e-file (e1..e8), white queen on e1, black pawn on e7, with empty intermediate squares
/// so a direct multi-step path capture can be validated without moving auxiliary pawns.
/// </summary>
internal sealed class ChessCaptureScenarioBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "chess-capture-mini";
        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);
        AddDirection(Constants.Directions.North); // increasing rank toward black side
        AddDirection(Constants.Directions.South); // toward white side

        // Build linear e-file: e1 (rank1) up to e8
        for (int r = 1; r <= 8; r++)
        {
            var tileId = $"tile-e{r}";
            var tile = AddTile(tileId);
            if (r > 1)
            {
                tile.WithRelationTo($"tile-e{r - 1}").InDirection(Constants.Directions.South);
            }
            if (r < 8)
            {
                tile.WithRelationTo($"tile-e{r + 1}").InDirection(Constants.Directions.North);
            }
        }

        // Pieces (queen sliding north/south; target pawn stationary facing south for consistency though direction irrelevant here)
        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteQueen)
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
                .HasDirection(Constants.Directions.North).CanRepeat()
                .HasDirection(Constants.Directions.South).CanRepeat();

        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn5)
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
                .HasDirection(Constants.Directions.South);

        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteQueen).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn5).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E7);

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