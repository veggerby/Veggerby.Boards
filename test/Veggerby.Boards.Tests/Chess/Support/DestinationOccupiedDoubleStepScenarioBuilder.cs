using Veggerby.Boards;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
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

        AddPlayer("white");
        AddPlayer("black");

        WithActivePlayer("white", true);
        WithActivePlayer("black", false);

        AddDirection("north");
        AddDirection("east");
        AddDirection("south");
        AddDirection("west");
        AddDirection("north-east");
        AddDirection("north-west");
        AddDirection("south-east");
        AddDirection("south-west");

        static string FileChar(int idx) => ((char)('a' + idx - 1)).ToString();
        for (var file = 1; file <= 8; file++)
        {
            for (var rank = 1; rank <= 8; rank++)
            {
                var id = $"tile-{FileChar(file)}{rank}";
                var tile = AddTile(id);
                if (file < 8) { tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank}").InDirection("east"); }
                if (file > 1) { tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank}").InDirection("west"); }
                if (rank < 8) { tile.WithRelationTo($"tile-{FileChar(file)}{rank + 1}").InDirection("north"); }
                if (rank > 1) { tile.WithRelationTo($"tile-{FileChar(file)}{rank - 1}").InDirection("south"); }
                if (file < 8 && rank < 8) { tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank + 1}").InDirection("north-east"); }
                if (file > 1 && rank < 8) { tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank + 1}").InDirection("north-west"); }
                if (file < 8 && rank > 1) { tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank - 1}").InDirection("south-east"); }
                if (file > 1 && rank > 1) { tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank - 1}").InDirection("south-west"); }
            }
        }

        AddPiece("white-pawn-test")
            .WithOwner("white")
            .HasDirection("north").Done()
            .HasPattern("north")
            .HasPattern("north", "north")
            .HasDirection("north-east").Done()
            .HasDirection("north-west").Done();

        // Opponent piece placed on e4 (double-step destination from e2)
        AddPiece("black-bishop-blocker")
            .WithOwner("black")
            .HasDirection("north").Done();

        WithPiece("white-pawn-test").OnTile("tile-e2");
        WithPiece("black-bishop-blocker").OnTile("tile-e4");

        WithState(new ChessStateExtras(true, true, true, true, null, 0, 1, System.Array.Empty<string>()));

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