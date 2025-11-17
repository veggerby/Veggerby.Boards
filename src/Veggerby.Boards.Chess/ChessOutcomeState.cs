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
    /// Initializes a new instance of the <see cref="ChessOutcomeState"/> class.
    /// </summary>
    /// <param name="status">The terminal condition.</param>
    /// <param name="winner">The winning player (null for stalemate).</param>
    public ChessOutcomeState(EndgameStatus status, Player? winner)
    {
        if (status == EndgameStatus.InProgress || status == EndgameStatus.Check)
        {
            throw new ArgumentException("ChessOutcomeState can only be created for terminal conditions (Checkmate or Stalemate)", nameof(status));
        }

        Status = status;
        Winner = winner;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is ChessOutcomeState cos && cos.Status == Status && Equals(cos.Winner, Winner);
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
                
                // We need to get both players from the winner reference
                // For stalemate, we can only represent the outcome if we know both players
                // Since we don't store all players here, we'll return minimal result
                // This will be populated by the GetOutcome() helper which has access to the full game
                return players;
            }

            if (Winner is not null)
            {
                // Checkmate - winner at rank 1
                return new[]
                {
                    new PlayerResult
                    {
                        Player = Winner,
                        Outcome = OutcomeType.Win,
                        Rank = 1
                    }
                };
            }

            return Array.Empty<PlayerResult>();
        }
    }
}
