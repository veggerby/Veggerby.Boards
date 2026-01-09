using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Represents the outcome of a Rock-Paper-Scissors game.
/// </summary>
public sealed class RockPaperScissorsOutcome : IGameOutcome
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RockPaperScissorsOutcome"/> class.
    /// </summary>
    /// <param name="winner">The winning player, or null if it's a tie.</param>
    /// <param name="player1">First player.</param>
    /// <param name="player1Choice">First player's choice.</param>
    /// <param name="player2">Second player.</param>
    /// <param name="player2Choice">Second player's choice.</param>
    public RockPaperScissorsOutcome(
        Player? winner,
        Player player1,
        Choice player1Choice,
        Player player2,
        Choice player2Choice)
    {
        Winner = winner;
        Player1 = player1;
        Player1Choice = player1Choice;
        Player2 = player2;
        Player2Choice = player2Choice;
    }

    /// <summary>
    /// Gets the winning player, or null if it's a tie.
    /// </summary>
    public Player? Winner { get; }

    /// <summary>
    /// Gets the first player.
    /// </summary>
    public Player Player1 { get; }

    /// <summary>
    /// Gets the first player's choice.
    /// </summary>
    public Choice Player1Choice { get; }

    /// <summary>
    /// Gets the second player.
    /// </summary>
    public Player Player2 { get; }

    /// <summary>
    /// Gets the second player's choice.
    /// </summary>
    public Choice Player2Choice { get; }

    /// <summary>
    /// Gets whether the game ended in a tie.
    /// </summary>
    public bool IsTie => Winner is null;

    /// <inheritdoc />
    public string TerminalCondition => IsTie ? "Draw" : "Winner";

    /// <summary>
    /// Determines the winner of a Rock-Paper-Scissors game based on player choices.
    /// </summary>
    /// <param name="state">The game state containing player choices.</param>
    /// <param name="player1">First player.</param>
    /// <param name="player2">Second player.</param>
    /// <returns>The game outcome.</returns>
    public static RockPaperScissorsOutcome DetermineWinner(GameState state, Player player1, Player player2)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(player1);
        ArgumentNullException.ThrowIfNull(player2);

        var choices = state.GetStates<PlayerChoiceState>().ToList();
        var choice1State = choices.FirstOrDefault(c => c.Artifact.Equals(player1));
        var choice2State = choices.FirstOrDefault(c => c.Artifact.Equals(player2));

        if (choice1State is null || choice2State is null)
        {
            throw new InvalidOperationException("Both players must have made a choice");
        }

        var choice1 = choice1State.Choice;
        var choice2 = choice2State.Choice;

        Player? winner = null;

        if (choice1 == choice2)
        {
            winner = null;
        }
        else if (choice1 == Choice.Rock && choice2 == Choice.Scissors)
        {
            winner = player1;
        }
        else if (choice1 == Choice.Rock && choice2 == Choice.Paper)
        {
            winner = player2;
        }
        else if (choice1 == Choice.Paper && choice2 == Choice.Rock)
        {
            winner = player1;
        }
        else if (choice1 == Choice.Paper && choice2 == Choice.Scissors)
        {
            winner = player2;
        }
        else if (choice1 == Choice.Scissors && choice2 == Choice.Paper)
        {
            winner = player1;
        }
        else if (choice1 == Choice.Scissors && choice2 == Choice.Rock)
        {
            winner = player2;
        }

        return new RockPaperScissorsOutcome(winner, player1, choice1, player2, choice2);
    }

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var results = new List<PlayerResult>();

            if (IsTie)
            {
                results.Add(new PlayerResult
                {
                    Player = Player1,
                    Outcome = OutcomeType.Draw,
                    Rank = 1
                });
                results.Add(new PlayerResult
                {
                    Player = Player2,
                    Outcome = OutcomeType.Draw,
                    Rank = 1
                });
            }
            else if (Winner!.Equals(Player1))
            {
                results.Add(new PlayerResult
                {
                    Player = Player1,
                    Outcome = OutcomeType.Win,
                    Rank = 1
                });
                results.Add(new PlayerResult
                {
                    Player = Player2,
                    Outcome = OutcomeType.Loss,
                    Rank = 2
                });
            }
            else
            {
                results.Add(new PlayerResult
                {
                    Player = Player1,
                    Outcome = OutcomeType.Loss,
                    Rank = 2
                });
                results.Add(new PlayerResult
                {
                    Player = Player2,
                    Outcome = OutcomeType.Win,
                    Rank = 1
                });
            }

            return results;
        }
    }
}
