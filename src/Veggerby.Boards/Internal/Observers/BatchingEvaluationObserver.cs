using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Observers;

/// <summary>
/// Decorator that batches high-frequency evaluation callbacks before forwarding to an inner <see cref="IEvaluationObserver"/>.
/// </summary>
/// <remarks>
/// Batching reduces virtual dispatch overhead when many rules are evaluated per event. Ordering across all callbacks for a
/// given handled event is preserved exactly. Flush triggers:
/// 1. Rule applied (success path)
/// 2. Event ignored (no rule applied)
/// 3. Buffer capacity reached (rare fallback)
/// A small struct buffer is used to avoid per-callback allocations. Buffer size chosen to cover common evaluation fan-out.
/// Large plans will segment into multiple flushes but still retain ordering guarantees.
/// </remarks>
internal sealed class BatchingEvaluationObserver(IEvaluationObserver inner, int capacity = BatchingEvaluationObserver.DefaultCapacity) : IEvaluationObserver
{
    private readonly IEvaluationObserver _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    // Event kinds; store minimal discriminated union for callback data
    private enum Kind : byte
    {
        PhaseEnter = 1,
        RuleEvaluated = 2,
        RuleApplied = 3,
        EventIgnored = 4,
        StateHashed = 5,
        RuleSkipped = 6,
    }

    private struct Entry
    {
        public Kind K;
        public GamePhase Phase;
        public IGameEventRule Rule;
        public ConditionResponse Response;
        public IGameEvent Event;
        public GameState Before;
        public GameState After;
        public int RuleIndex;
        public RuleSkipReason SkipReason;
        public ulong Hash;
        public GameState State;
    }

    private readonly Entry[] _buffer = new Entry[capacity <= 8 ? DefaultCapacity : capacity];
    private int _count = 0;
    private const int DefaultCapacity = 128; // heuristic: most event evaluations far below this

    private void Add(in Entry e)
    {
        if (_count == _buffer.Length)
        {
            Flush(); // capacity reached (rare)
        }
        _buffer[_count++] = e;
    }

    private void Flush()
    {
        for (var i = 0; i < _count; i++)
        {
            ref var e = ref _buffer[i];
            switch (e.K)
            {
                case Kind.PhaseEnter:
                    _inner.OnPhaseEnter(e.Phase, e.State);
                    break;
                case Kind.RuleEvaluated:
                    _inner.OnRuleEvaluated(e.Phase, e.Rule, e.Response, e.State, e.RuleIndex);
                    break;
                case Kind.RuleApplied:
                    _inner.OnRuleApplied(e.Phase, e.Rule, e.Event, e.Before, e.After, e.RuleIndex);
                    break;
                case Kind.EventIgnored:
                    _inner.OnEventIgnored(e.Event, e.State);
                    break;
                case Kind.StateHashed:
                    _inner.OnStateHashed(e.State, e.Hash);
                    break;
                case Kind.RuleSkipped:
                    _inner.OnRuleSkipped(e.Phase, e.Rule, e.SkipReason, e.State, e.RuleIndex);
                    break;
            }
        }
        _count = 0;
    }

    private void FlushIfTerminal()
    {
        // Terminal markers (RuleApplied, EventIgnored) always at end logically – flush now to publish batch.
        Flush();
    }

    public void OnPhaseEnter(GamePhase phase, GameState state)
    {
        Add(new Entry { K = Kind.PhaseEnter, Phase = phase, State = state });
    }

    public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
    {
        Add(new Entry { K = Kind.RuleEvaluated, Phase = phase, Rule = rule, Response = response, State = state, RuleIndex = ruleIndex });
    }

    public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex)
    {
        Add(new Entry { K = Kind.RuleApplied, Phase = phase, Rule = rule, Event = @event, Before = beforeState, After = afterState, RuleIndex = ruleIndex });
        FlushIfTerminal();
    }

    public void OnEventIgnored(IGameEvent @event, GameState state)
    {
        Add(new Entry { K = Kind.EventIgnored, Event = @event, State = state });
        FlushIfTerminal();
    }

    public void OnStateHashed(GameState state, ulong hash)
    {
        Add(new Entry { K = Kind.StateHashed, State = state, Hash = hash });
    }

    public void OnRuleSkipped(GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex)
    {
        Add(new Entry { K = Kind.RuleSkipped, Phase = phase, Rule = rule, SkipReason = reason, State = state, RuleIndex = ruleIndex });
    }
}