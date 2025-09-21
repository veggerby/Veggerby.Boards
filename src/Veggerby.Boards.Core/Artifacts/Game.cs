using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts;

/// <summary>
/// Immutable snapshot of structural game composition (board, players, artifacts definitions).
/// Dynamic state (positions, dice values) lives in <c>GameState</c> instances.
/// </summary>
public class Game
{
    /// <summary>
    /// Gets the board.
    /// </summary>
    public Board Board { get; }
    /// <summary>
    /// Gets the players.
    /// </summary>
    public IEnumerable<Player> Players { get; }
    /// <summary>
    /// Gets all artifacts (including tiles, pieces, dice, etc.).
    /// </summary>
    public IEnumerable<Artifact> Artifacts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game(Board board, IEnumerable<Player> players, IEnumerable<Artifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(board);

        ArgumentNullException.ThrowIfNull(players);

        if (!players.Any())
        {
            throw new ArgumentException("Empty player list", nameof(players));
        }

        ArgumentNullException.ThrowIfNull(artifacts);

        if (!artifacts.Any())
        {
            throw new ArgumentException("Empty piece list", nameof(artifacts));
        }

        Board = board;
        Players = players.ToList().AsReadOnly();
        Artifacts = artifacts.ToList().AsReadOnly();
    }
}