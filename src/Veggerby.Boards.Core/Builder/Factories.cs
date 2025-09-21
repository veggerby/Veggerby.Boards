using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder;

/// <summary>
/// Factory delegate for producing a game state condition instance from a compiled <see cref="Game"/>.
/// </summary>
/// <param name="game">The compiled game context.</param>
/// <returns>An initialized game state condition.</returns>
public delegate IGameStateCondition GameStateConditionFactory(Game game);

/// <summary>
/// Factory delegate for producing a game event condition instance from a compiled <see cref="Game"/>.
/// </summary>
/// <typeparam name="T">Concrete event type the condition evaluates.</typeparam>
/// <param name="game">The compiled game context.</param>
/// <returns>An initialized game event condition.</returns>
public delegate IGameEventCondition<T> GameEventConditionFactory<T>(Game game) where T : IGameEvent;

/// <summary>
/// Factory delegate for producing a state mutator for a specific event type.
/// </summary>
/// <typeparam name="T">Concrete event type the mutator consumes.</typeparam>
/// <param name="game">The compiled game context.</param>
/// <returns>An initialized state mutator.</returns>
public delegate IStateMutator<T> StateMutatorFactory<T>(Game game) where T : IGameEvent;

/// <summary>
/// Factory delegate for producing an event pre-processor instance.
/// </summary>
/// <param name="game">The compiled game context.</param>
/// <returns>A pre-processor implementation.</returns>
public delegate IGameEventPreProcessor GameEventPreProcessorFactory(Game game);