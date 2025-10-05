namespace Veggerby.Boards.Simulation;

#nullable enable

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

/// <summary>
/// Delegate signature for selecting the next event to attempt during a simulation playout.
/// </summary>
/// <param name="state">Current immutable game state snapshot.</param>
/// <returns>An <see cref="T:Veggerby.Boards.Flows.Events.IGameEvent"/> to attempt, or <c>null</c> to signal no further legal moves / end of playout.</returns>
public delegate IGameEvent? PlayoutPolicy(GameState state);

/// <summary>
/// Delegate signature controlling early termination of a playout.
/// </summary>
/// <param name="initialState">Initial game state at start of playout.</param>
/// <param name="currentState">Current game state.</param>
/// <param name="depth">Number of successfully applied events so far.</param>
/// <returns><c>true</c> to stop the playout, otherwise <c>false</c>.</returns>
public delegate bool PlayoutStopPredicate(GameState initialState, GameState currentState, int depth);

#nullable disable