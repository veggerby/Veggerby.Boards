using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Infrastructure;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Core;

public class DecisionPlanParityTests
{
    private static GameProgress Build(bool enablePlan)
    {
        using var _ = new FeatureFlagScope(decisionPlan: enablePlan);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        return builder.Compile();
    }

    [Fact]
    public void MovePiece_Should_have_identical_end_state_with_and_without_decision_plan()
    {
        // arrange
        var legacy = Build(false);
        var plan = Build(true);
        var piece = legacy.Game.GetPiece("piece-1");
        var from = legacy.Game.GetTile("tile-1");
        var to = legacy.Game.GetTile("tile-2");
        var relation = legacy.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new TilePath([relation]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var legacyResult = legacy.HandleEvent(evt);
        var planResult = plan.HandleEvent(evt);

        // assert
        var legacyPieceState = legacyResult.State.GetState<PieceState>(piece);
        var planPieceState = planResult.State.GetState<PieceState>(piece);
        planPieceState.CurrentTile.Should().Be(legacyPieceState.CurrentTile);
    }

    [Fact]
    public void RollDice_Should_have_identical_end_state_with_and_without_decision_plan()
    {
        // arrange
        var legacy = Build(false);
        var plan = Build(true);
        var dice = legacy.Game.GetArtifact<Dice>("dice");
        // create a deterministic dice state (value arbitrary for parity, both paths receive same value)
        var newState = new DiceState<int>(dice, 5);
        var evt = new RollDiceGameEvent<int>(newState);

        // act
        var legacyResult = legacy.HandleEvent(evt);
        var planResult = plan.HandleEvent(evt);

        // assert
        var legacyDiceState = legacyResult.State.GetState<DiceState<int>>(dice);
        var planDiceState = planResult.State.GetState<DiceState<int>>(dice);
        planDiceState.CurrentValue.Should().Be(legacyDiceState.CurrentValue);
    }

    [Fact]
    public void MultiEventSequence_Should_remain_in_parity()
    {
        // arrange
        var legacy = Build(false);
        var plan = Build(true);

        var piece1 = legacy.Game.GetPiece("piece-1");
        var tile1 = legacy.Game.GetTile("tile-1");
        var tile2 = legacy.Game.GetTile("tile-2");
        var relation = legacy.Game.Board.TileRelations.Single(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        var movePath = new TilePath([relation]);
        var moveEvent = new MovePieceGameEvent(piece1, movePath);

        var dice = legacy.Game.GetArtifact<Dice>("dice");
        var diceEvent = new RollDiceGameEvent<int>(new DiceState<int>(dice, 3));

        // act (apply sequence: roll -> move -> roll)
        var legacyAfterFirstRoll = legacy.HandleEvent(diceEvent);
        var planAfterFirstRoll = plan.HandleEvent(diceEvent);

        var legacyAfterMove = legacyAfterFirstRoll.HandleEvent(moveEvent);
        var planAfterMove = planAfterFirstRoll.HandleEvent(moveEvent);

        var legacyAfterSecondRoll = legacyAfterMove.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(dice, 6)));
        var planAfterSecondRoll = planAfterMove.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(dice, 6)));

        // assert piece location parity
        var legacyPieceState = legacyAfterSecondRoll.State.GetState<PieceState>(piece1);
        var planPieceState = planAfterSecondRoll.State.GetState<PieceState>(piece1);
        planPieceState.CurrentTile.Should().Be(legacyPieceState.CurrentTile);

        // assert dice value parity
        var legacyDiceState = legacyAfterSecondRoll.State.GetState<DiceState<int>>(dice);
        var planDiceState = planAfterSecondRoll.State.GetState<DiceState<int>>(dice);
        planDiceState.CurrentValue.Should().Be(legacyDiceState.CurrentValue);
    }
}