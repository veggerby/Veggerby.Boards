using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Fakes;

internal class MaskingTestGameBuilder : GameBuilder
{
    protected override void Build()
    {
        // Board 2x1 with east direction
        BoardId = "board";
        AddDirection(Constants.Directions.East);
        AddTile("tile-1-1");
        AddTile("tile-2-1");
        WithTile("tile-1-1").WithRelationTo("tile-2-1").InDirection(Constants.Directions.East);
        AddPlayer("p1");
        AddPlayer("p2");
        AddPiece("a").OnTile("tile-1-1").WithOwner("p1").HasDirection(Constants.Directions.East);
        AddPiece("b").OnTile("tile-1-1").WithOwner("p2").HasDirection(Constants.Directions.East);

        AddGamePhase("phase-a").Exclusive("g1")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<MovePieceGameEvent>()
                .Then()
                    .Do<MovePieceStateMutator>();
        AddGamePhase("phase-b").Exclusive("g1")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<MovePieceGameEvent>()
                .Then()
                    .Do<MovePieceStateMutator>();
    }
}