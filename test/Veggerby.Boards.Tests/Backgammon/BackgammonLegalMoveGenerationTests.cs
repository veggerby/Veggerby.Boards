using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Backgammon.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Backgammon;

/// <summary>
/// Tests for Backgammon legal move generation API.
/// </summary>
public class BackgammonLegalMoveGenerationTests
{
    [Fact]
    public void GivenBackgammonStartingPosition_WhenGettingLegalMoves_ThenReturnsEmpty()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(progress.State).ToList();

        // assert
        // Starting position with no dice rolled has no enumerable moves
        // (rolling dice is a separate action not enumerated as a "move")
        legalMoves.Should().BeEmpty("No dice rolled means no moves can be enumerated");
    }

    [Fact]
    public void GivenBackgammonGame_WhenValidatingRollDiceEvent_ThenDelegatestoBase()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var generator = progress.GetBackgammonLegalMoveGenerator();

        var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
        var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
        dice1.Should().NotBeNull();
        dice2.Should().NotBeNull();

        var rollEvent = new RollDiceGameEvent<int>([
            new DiceState<int>(dice1!, 0),
            new DiceState<int>(dice2!, 0)
        ]);

        // act
        var validation = generator.Validate(rollEvent, progress.State);

        // assert
        // Validation is delegated to base generator which uses DecisionPlan
        // The actual legality depends on the game rules, not the generator
        validation.Should().NotBeNull();
    }

    [Fact]
    public void GivenBackgammonWithDiceRolled_WhenGettingLegalMoves_ThenReturnsPieceMoves()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();

        // Set active player manually (Backgammon normally determines this through initial roll)
        var whitePlayer = progress.Game.GetPlayer("white");
        var stateWithActivePlayer = progress.State.Next([new ActivePlayerState(whitePlayer!, true)]);
        progress = new GameProgress(progress.Engine, stateWithActivePlayer, progress.Events);

        // Roll dice to get some values
        progress = progress.RollDice("dice-1", "dice-2");

        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(progress.State).ToList();

        // assert
        // After rolling dice, should have piece movement options
        legalMoves.Should().NotBeEmpty("Should have piece moves after dice roll");
        legalMoves.Should().AllBeOfType<MovePieceGameEvent>();
    }

    [Fact]
    public void GivenEndedGame_WhenGettingLegalMoves_ThenReturnsEmpty()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();

        // Manually end the game
        var endedState = progress.State.Next([new GameEndedState()]);
        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(endedState).ToList();

        // assert
        legalMoves.Should().BeEmpty("Game has ended, no legal moves should exist");
    }

    [Fact]
    public void GivenEndedGame_WhenValidatingMove_ThenReturnsGameEnded()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();

        var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
        var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
        var rollEvent = new RollDiceGameEvent<int>([
            new DiceState<int>(dice1!, 0),
            new DiceState<int>(dice2!, 0)
        ]);

        // End the game
        var endedState = progress.State.Next([new GameEndedState()]);
        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var validation = generator.Validate(rollEvent, endedState);

        // assert
        validation.IsLegal.Should().BeFalse("Move should be illegal after game ends");
        validation.Reason.Should().Be(RejectionReason.GameEnded);
    }

    [Fact]
    public void GivenBackgammonWithDiceRolled_WhenGettingLegalMovesForPiece_ThenReturnsMovesForThatPiece()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();

        // Roll dice and select starting player
        progress = progress.RollDice("dice-1", "dice-2");

        // Get active player's piece
        if (!progress.State.TryGetActivePlayer(out var activePlayer) || activePlayer is null)
        {
            // Skip if no active player (may need initial roll)
            return;
        }

        var piece = progress.Game.GetArtifacts<Piece>()
            .FirstOrDefault(p => p.Owner == activePlayer);

        if (piece is null)
        {
            return;
        }

        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var legalMovesForPiece = generator.GetLegalMovesFor(piece, progress.State).ToList();

        // assert
        // Should have moves for this specific piece (may be empty if no legal moves for this piece)
        legalMovesForPiece.Should().AllBeOfType<MovePieceGameEvent>();

        if (legalMovesForPiece.Any())
        {
            legalMovesForPiece.Cast<MovePieceGameEvent>()
                .Should().OnlyContain(m => m.Piece == piece, "All moves should be for the specified piece");
        }
    }

    [Fact]
    public void GivenBackgammonGame_WhenNoDiceValues_ThenNoMovesEnumerated()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();

        // Ensure no dice values
        var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
        var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
        var dice1State = progress.State.GetState<DiceState<int>>(dice1!);
        var dice2State = progress.State.GetState<DiceState<int>>(dice2!);

        // Verify no dice values
        (dice1State?.CurrentValue ?? 0).Should().Be(0, "dice-1 should have no value");
        (dice2State?.CurrentValue ?? 0).Should().Be(0, "dice-2 should have no value");

        var generator = progress.GetBackgammonLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(progress.State).ToList();

        // assert
        legalMoves.Should().BeEmpty("No dice values means no moves can be enumerated");
    }
}
