using System;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition for a dice artifact including initial value state.
/// </summary>
public class DiceDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured dice identifier.
    /// </summary>
    public string DiceId { get; private set; }

    /// <summary>
    /// Sets the dice identifier.
    /// </summary>
    /// <param name="id">Unique dice id.</param>
    /// <returns>The same definition for chaining.</returns>
    public DiceDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        DiceId = id;
        return this;
    }

    /// <summary>
    /// Initializes the dice without a current value.
    /// </summary>
    public DiceDefinition HasNoValue()
    {
        Builder.AddDiceState(DiceId, null);
        return this;
    }

    /// <summary>
    /// Initializes the dice with a starting numeric value.
    /// </summary>
    /// <param name="value">Initial face value.</param>
    public DiceDefinition HasValue(int value)
    {
        Builder.AddDiceState(DiceId, value);
        return this;
    }
}