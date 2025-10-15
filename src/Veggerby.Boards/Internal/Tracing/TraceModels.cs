using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Tracing;

/// <summary>
/// A single evaluation trace entry capturing a deterministic step during event handling.
/// </summary>
#nullable enable
internal sealed record TraceEntry(
    int Order,
    string Kind,
    string? Phase,
    string? Rule,
    string? EventType,
    string? ConditionResult,
    string? ConditionReason,
    int? RuleIndex,
    ulong? StateHash,
    ulong? StateHash128Low,
    ulong? StateHash128High
);

/// <summary>
/// Container for the most recent evaluation trace.
/// </summary>
internal sealed class EvaluationTrace
{
    private readonly List<TraceEntry> _entries = [];
    public IReadOnlyList<TraceEntry> Entries => _entries;

    public void Add(TraceEntry entry) => _entries.Add(entry);

    public string ToJson()
    {
        return TraceJsonExporter.Serialize(this, indented: false);
    }
}

/// <summary>
/// Deterministic JSON exporter for <see cref="EvaluationTrace"/>.
/// </summary>
/// <remarks>
/// Keeps a stable field ordering and omits null fields for compactness. Intended for diagnostics only; schema may evolve.
/// </remarks>
internal static class TraceJsonExporter
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(EvaluationTrace trace, bool indented = false)
    {
        // Defensive: ensure entries already in deterministic order (append-only with Order). We rely on insertion order here.
        return JsonSerializer.Serialize(trace.Entries, indented ? IndentedOptions : CompactOptions);
    }
}

/// <summary>
/// Observer decorator that records evaluation steps to an in-memory trace when trace capture is enabled.
/// </summary>
internal sealed class TraceCaptureObserver(IEvaluationObserver inner, EvaluationTrace trace) : IEvaluationObserver
{
    private readonly IEvaluationObserver _inner = inner;
    private readonly EvaluationTrace _trace = trace;
    private int _order;

    public void OnPhaseEnter(GamePhase phase, GameState state)
    {
        _trace.Add(new TraceEntry(++_order, "PhaseEnter", phase.Label, null, null, null, null, null, state.Hash, state.Hash128?.Low, state.Hash128?.High));
        _inner.OnPhaseEnter(phase, state);
    }

    public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
    {
        _trace.Add(new TraceEntry(++_order, "RuleEvaluated", phase.Label, rule.GetType().Name, null, response.Result.ToString(), response.Reason, ruleIndex, state.Hash, state.Hash128?.Low, state.Hash128?.High));
        _inner.OnRuleEvaluated(phase, rule, response, state, ruleIndex);
    }

    public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex)
    {
        _trace.Add(new TraceEntry(++_order, "RuleApplied", phase.Label, rule.GetType().Name, @event.GetType().Name, null, null, ruleIndex, afterState.Hash, afterState.Hash128?.Low, afterState.Hash128?.High));
        _inner.OnRuleApplied(phase, rule, @event, beforeState, afterState, ruleIndex);
    }

    public void OnEventIgnored(IGameEvent @event, GameState state)
    {
        _trace.Add(new TraceEntry(++_order, "EventIgnored", null, null, @event.GetType().Name, null, null, null, state.Hash, state.Hash128?.Low, state.Hash128?.High));
        _inner.OnEventIgnored(@event, state);
    }

    public void OnStateHashed(GameState state, ulong hash)
    {
        _trace.Add(new TraceEntry(++_order, "StateHashed", null, null, null, null, null, null, hash, state.Hash128?.Low, state.Hash128?.High));
        _inner.OnStateHashed(state, hash);
    }

    public void OnRuleSkipped(GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex)
    {
        _trace.Add(new TraceEntry(++_order, "RuleSkipped", phase?.Label, rule?.GetType().Name, null, null, reason.ToString(), ruleIndex, state.Hash, state.Hash128?.Low, state.Hash128?.High));
        // IEvaluationObserver contract currently requires non-null phase and rule. If either is null we only record the trace entry
        // (for diagnostics) and suppress the downstream callback to avoid violating nullability. Engine sources should ideally never
        // propagate null here; this guard preserves safety while retaining trace fidelity.
        if (phase is not null && rule is not null)
        {
            _inner.OnRuleSkipped(phase, rule, reason, state, ruleIndex);
        }
    }
}