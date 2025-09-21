using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Events;

/// <summary>
/// Placeholder discriminated style result for event handling to support future observer/tracing integration.
/// </summary>
/// <remarks>
/// Not yet wired into public APIs; current pipeline still returns <see cref="GameProgress"/> directly.
/// This struct provides a forward-compatible location to capture metadata (rule index, phase number,
/// trace id, hash) when feature flags enable richer diagnostics.
/// </remarks>
public readonly record struct EventResult(GameState State, bool Applied);