using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Marker artifact for chess game outcome.
/// </summary>
internal sealed class ChessOutcomeMarker : Artifact
{
    public ChessOutcomeMarker() : base("chess-outcome-marker") { }
}

/// <summary>
/// Immutable state representing the final outcome of a chess game.
/// </summary>
/// <remarks>
/// This state is added to the game when a terminal condition is detected (checkmate or stalemate).
/// It implements <see cref="IGameOutcome"/> to provide standardized outcome information.
/// </remarks>
public sealed class ChessOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly ChessOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the type of terminal condition that ended the game.
    /// </summary>
    public EndgameStatus Status { get; }

    /// <summary>
    /// Gets the player who won (null for stalemate/draw).
    /// </summary>
    public Player? Winner { get; }

    /// <summary>
    /// Gets the player who lost (null for stalemate/draw).
    /// </summary>
    public Player? Loser { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessOutcomeState"/> class.
    /// </summary>
    /// <param name="status">The terminal condition.</param>
    /// <param name="winner">The winning player (null for stalemate).</param>
    /// <param name="loser">The losing player (null for stalemate).</param>
    public ChessOutcomeState(EndgameStatus status, Player? winner, Player? loser = null)
    {
        if (status == EndgameStatus.InProgress || status == EndgameStatus.Check)
        {
            throw new ArgumentException("ChessOutcomeState can only be created for terminal conditions (Checkmate or Stalemate)", nameof(status));
        }

        Status = status;
        Winner = winner;
        Loser = loser;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is ChessOutcomeState cos && cos.Status == Status && Equals(cos.Winner, Winner) && Equals(cos.Loser, Loser);
    }

    /// <inheritdoc />
    public string TerminalCondition => Status.ToString();

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            if (Status == EndgameStatus.Stalemate)
            {
                // Stalemate is a draw - both players tied at rank 1
                var players = new List<PlayerResult>();

                if (Winner != null)
                {
                    players.Add(new PlayerResult
                    {
                        Player = Winner,
                        Outcome = OutcomeType.Draw,
                        Rank = 1
                    });
                }

                if (Loser != null)
                {
                    players.Add(new PlayerResult
                    {
                        Player = Loser,
                        Outcome = OutcomeType.Draw,
                        Rank = 1
                    });
                }

                return players;
            }

            if (Winner is not null)
            {
                var results = new List<PlayerResult>
                {
                    new PlayerResult
                    {
                        Player = Winner,
                        Outcome = OutcomeType.Win,
                        Rank = 1
                    }
                };

                if (Loser is not null)
                {
                    results.Add(new PlayerResult
                    {
                        Player = Loser,
                        Outcome = OutcomeType.Loss,
                        Rank = 2
                    });
                }

                return results;
            }

            return Array.Empty<PlayerResult>();
        }
    }
}
