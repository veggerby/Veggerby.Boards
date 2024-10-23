using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts;

public class Game
{
    public Board Board { get; }
    public IEnumerable<Player> Players { get; }
    public IEnumerable<Artifact> Artifacts { get; }

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