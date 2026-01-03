using System;
using System.Linq;

using Veggerby.Boards.Othello;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Othello;

public class OthelloEndgameTests
{
    [Fact]
    public void Should_detect_game_end_when_board_is_full()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        // Simulate a full board by placing discs on all tiles
        // This is a simplified test - in reality we'd need valid moves
        // For now, just verify the endgame condition logic works

        // Fill the board artificially for testing
        var allTiles = progress.Game.Board.Tiles.ToList();
        var pieceStates = new System.Collections.Generic.List<IArtifactState>();

        for (int i = 0; i < allTiles.Count; i++)
        {
            var pieceId = i < 32 ? $"black-disc-{i + 1}" : $"white-disc-{i - 31}";
            var piece = progress.Game.GetPiece(pieceId);
            if (piece != null && !progress.State.GetPiecesOnTile(allTiles[i]).Any())
            {
                pieceStates.Add(new PieceState(piece, allTiles[i]));
            }
        }

        var fullBoardState = progress.State.Next(pieceStates.ToArray());

        // act
        var condition = new Veggerby.Boards.Othello.Conditions.OthelloEndgameCondition(progress.Game);
        var result = condition.Evaluate(fullBoardState);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Should_count_discs_correctly_at_game_end()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        // Place a disc to test counting
        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        progress = progress.HandleEvent(new PlaceDiscGameEvent(disc, tile));

        // Manually trigger endgame
        var endgameMutator = new Veggerby.Boards.Othello.Mutators.OthelloEndGameMutator(progress.Game);
        var endedState = endgameMutator.MutateState(progress.Engine, progress.State, new PassTurnGameEvent());

        // act
        var outcomeState = endedState.GetStates<OthelloOutcomeState>().FirstOrDefault();

        // assert
        outcomeState.Should().NotBeNull();
        outcomeState!.BlackScore.Should().Be(4);
        outcomeState.WhiteScore.Should().Be(1);
        outcomeState.Winner!.Id.Should().Be(OthelloIds.Players.Black);
    }

    [Fact]
    public void Should_determine_winner_correctly_when_black_has_more_discs()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        // Place a disc to give black more discs
        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        progress = progress.HandleEvent(new PlaceDiscGameEvent(disc, tile));

        // act
        var endgameMutator = new Veggerby.Boards.Othello.Mutators.OthelloEndGameMutator(progress.Game);
        var endedState = endgameMutator.MutateState(progress.Engine, progress.State, new PassTurnGameEvent());
        var outcomeState = endedState.GetStates<OthelloOutcomeState>().FirstOrDefault();

        // assert
        outcomeState.Should().NotBeNull();
        outcomeState!.Winner.Should().NotBeNull();
        outcomeState.Winner!.Id.Should().Be(OthelloIds.Players.Black);
    }

    [Fact]
    public void Should_create_GameEndedState_when_game_ends()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        progress = progress.HandleEvent(new PlaceDiscGameEvent(disc, tile));

        // act
        var endgameMutator = new Veggerby.Boards.Othello.Mutators.OthelloEndGameMutator(progress.Game);
        var endedState = endgameMutator.MutateState(progress.Engine, progress.State, new PassTurnGameEvent());

        // assert
        var gameEndedState = endedState.GetStates<GameEndedState>().FirstOrDefault();
        gameEndedState.Should().NotBeNull();
    }
}
