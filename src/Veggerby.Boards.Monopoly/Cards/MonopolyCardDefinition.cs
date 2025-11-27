using System;

namespace Veggerby.Boards.Monopoly.Cards;

/// <summary>
/// Defines a Monopoly card with its effect and parameters.
/// </summary>
public sealed class MonopolyCardDefinition
{
    /// <summary>
    /// Gets the card identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the card's display text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the effect type of the card.
    /// </summary>
    public MonopolyCardEffect Effect { get; }

    /// <summary>
    /// Gets the primary value parameter (position, amount, spaces).
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets an optional secondary value parameter (e.g., per-hotel amount for repairs).
    /// </summary>
    public int SecondaryValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyCardDefinition"/> class.
    /// </summary>
    public MonopolyCardDefinition(string id, string text, MonopolyCardEffect effect, int value = 0, int secondaryValue = 0)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);

        Id = id;
        Text = text;
        Effect = effect;
        Value = value;
        SecondaryValue = secondaryValue;
    }
}
