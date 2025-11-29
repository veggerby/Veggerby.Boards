using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Immutable snapshot of structural game composition (board, players, artifacts definitions).
/// Dynamic state (positions, dice values) lives in <c>GameState</c> instances.
/// </summary>
public class Game
{
    private readonly Dictionary<string, Player> _playerLookup;
    private readonly Dictionary<string, Artifact> _artifactLookup;

    /// <summary>
    /// Gets the board.
    /// </summary>
    public Board Board
    {
        get;
    }

    /// <summary>
    /// Gets the players.
    /// </summary>
    public IEnumerable<Player> Players
    {
        get;
    }

    /// <summary>
    /// Gets all artifacts (including tiles, pieces, dice, etc.).
    /// </summary>
    public IEnumerable<Artifact> Artifacts
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game(Board board, IEnumerable<Player> players, IEnumerable<Artifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(board);

        ArgumentNullException.ThrowIfNull(players);

        var playerList = players.ToList();
        if (playerList.Count == 0)
        {
            throw new ArgumentException("Empty player list", nameof(players));
        }

        ArgumentNullException.ThrowIfNull(artifacts);

        var artifactList = artifacts.ToList();
        if (artifactList.Count == 0)
        {
            throw new ArgumentException("Empty artifact list", nameof(artifacts));
        }

        Board = board;
        Players = playerList.AsReadOnly();
        Artifacts = artifactList.AsReadOnly();

        // Build O(1) lookup dictionaries for frequent ID-based access
        // Note: Duplicate IDs are intentionally allowed - last occurrence wins. This preserves
        // backward compatibility with existing behavior where duplicate IDs would not throw.
        _playerLookup = new Dictionary<string, Player>(playerList.Count, StringComparer.Ordinal);
        foreach (var player in playerList)
        {
            _playerLookup[player.Id] = player;
        }

        _artifactLookup = new Dictionary<string, Artifact>(artifactList.Count, StringComparer.Ordinal);
        foreach (var artifact in artifactList)
        {
            _artifactLookup[artifact.Id] = artifact;
        }
    }

    /// <summary>
    /// Retrieves a player by identifier using O(1) dictionary lookup.
    /// </summary>
    /// <param name="id">The player identifier.</param>
    /// <returns>The player if found; otherwise <c>null</c>.</returns>
    internal Player? GetPlayerById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return _playerLookup.TryGetValue(id, out var player) ? player : null;
    }

    /// <summary>
    /// Retrieves an artifact by identifier using O(1) dictionary lookup.
    /// </summary>
    /// <param name="id">The artifact identifier.</param>
    /// <returns>The artifact if found; otherwise <c>null</c>.</returns>
    internal Artifact? GetArtifactById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return _artifactLookup.TryGetValue(id, out var artifact) ? artifact : null;
    }
}