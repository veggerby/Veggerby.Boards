using System;


using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Logical negation wrapper for a child condition. Converts Valid -> Invalid, Invalid -> Valid, preserves Ignore.
/// </summary>
public class NotGameEventConditon<T> : IGameEventCondition<T> where T : IGameEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotGameEventConditon{T}"/> class.
    /// </summary>
    /// <param name="innerCondition">Condition to negate.</param>
    public NotGameEventConditon(IGameEventCondition<T> innerCondition)
    {
        ArgumentNullException.ThrowIfNull(innerCondition);

        InnerCondition = innerCondition;
    }

    /// <summary>
    /// Gets the wrapped condition.
    /// </summary>
    public IGameEventCondition<T> InnerCondition { get; }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        var innerResult = InnerCondition.Evaluate(engine, state, @event);
        switch (innerResult.Result)
        {
            case ConditionResult.Valid:
                return ConditionResponse.Fail(innerResult.Reason);
            case ConditionResult.Ignore:
                return innerResult;
            case ConditionResult.Invalid:
                return ConditionResponse.Success(innerResult.Reason);
        }

        throw new BoardException("Invalid response");
    }
}