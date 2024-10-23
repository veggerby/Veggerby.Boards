using System;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Phases;

namespace Veggerby.Boards.Core;

public class GameEngine
{
    public Game Game { get; }
    public GamePhase GamePhaseRoot { get; }

    public GameEngine(Game game, GamePhase gamePhaseRoot)
    {
        ArgumentNullException.ThrowIfNull(game);

        ArgumentNullException.ThrowIfNull(gamePhaseRoot);

        Game = game;
        GamePhaseRoot = gamePhaseRoot;
    }
}