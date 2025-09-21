using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Fakes;

public class TestGameBuilder(bool useSimpleGamePhase = true) : GameBuilder
{
    private readonly bool _useSimpleGamePhase = useSimpleGamePhase;

    protected override void Build()
    {
        // Game + state

        BoardId = "test";

        AddDice("dice").HasNoValue();
        AddDice("dice-secondary").HasValue(4);

        AddPlayer("player-1");
        AddPlayer("player-2");

        AddDirection("clockwise");
        AddDirection("counterclockwise");
        AddDirection("up");

        AddTile("tile-1").WithRelationTo("tile-2").InDirection("clockwise");
        AddTile("tile-2").WithRelationTo("tile-1").InDirection("counterclockwise");
        AddTile("tile-3").WithRelationFrom("tile-1").InDirection("up");

        AddPiece("piece-1").WithOwner("player-1").HasDirection("clockwise").CanRepeat().OnTile("tile-1");
        AddPiece("piece-2").WithOwner("player-2").HasDirection("counterclockwise").DoesNotRepeat().OnTile("tile-2");
        AddPiece("piece-n").OnTile("tile-1");
        AddPiece("piece-x")
            .HasDirection("clockwise").CanRepeat()
            .HasDirection("counterclockwise").CanRepeat();
        AddPiece("piece-y")
            .HasDirection("clockwise").DoesNotRepeat()
            .HasDirection("counterclockwise").DoesNotRepeat();

        AddArtifact("artifact-x").WithFactory(id => new TestArtifact(id));

        // game phase

        if (!_useSimpleGamePhase)
        {
            AddGamePhase("just a simple phase")
                .If<InitialGameStateCondition>()
                    .And(game => new DiceGameStateCondition<int>(game.GetArtifacts<Dice>(), CompositeMode.Any))
                .Then()
                    .ForEvent<RollDiceGameEvent<int>>()
                        .If(game => new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.Valid))
                            .Or(game => new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.NotApplicable))
                        .Then()
                            //.DoBefore<DiceStateMutator<int>>()
                            .Do<DiceStateMutator<int>>()
                    .ForEvent<MovePieceGameEvent>()
                        .If(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Valid))
                            .Or(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.NotApplicable))
                        .Then()
                            .Do<MovePieceStateMutator>();
        }
    }
}