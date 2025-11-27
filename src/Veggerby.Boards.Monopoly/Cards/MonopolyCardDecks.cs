using System.Collections.Generic;

namespace Veggerby.Boards.Monopoly.Cards;

/// <summary>
/// Provides the standard Monopoly Chance and Community Chest card definitions.
/// </summary>
public static class MonopolyCardDecks
{
    /// <summary>
    /// Gets the standard Chance deck (16 cards).
    /// </summary>
    public static IReadOnlyList<MonopolyCardDefinition> ChanceCards { get; } = new List<MonopolyCardDefinition>
    {
        // Movement cards
        new("chance-advance-go", "Advance to Go. Collect $200.", MonopolyCardEffect.AdvanceToPosition, 0),
        new("chance-advance-illinois", "Advance to Illinois Avenue.", MonopolyCardEffect.AdvanceToPosition, 24),
        new("chance-advance-stcharles", "Advance to St. Charles Place. If you pass Go, collect $200.", MonopolyCardEffect.AdvanceToPosition, 11),
        new("chance-advance-boardwalk", "Advance to Boardwalk.", MonopolyCardEffect.AdvanceToPosition, 39),
        new("chance-advance-reading", "Take a trip to Reading Railroad. If you pass Go, collect $200.", MonopolyCardEffect.AdvanceToPosition, 5),
        new("chance-go-back-3", "Go back 3 spaces.", MonopolyCardEffect.MoveBackward, 3),

        // Railroad/Utility cards
        new("chance-nearest-railroad-1", "Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay owner twice the rental to which they are otherwise entitled.", MonopolyCardEffect.AdvanceToNearestRailroad, 0),
        new("chance-nearest-railroad-2", "Advance to the nearest Railroad. If unowned, you may buy it from the Bank. If owned, pay owner twice the rental to which they are otherwise entitled.", MonopolyCardEffect.AdvanceToNearestRailroad, 0),
        new("chance-nearest-utility", "Advance to the nearest Utility. If unowned, you may buy it from the Bank. If owned, throw dice and pay owner a total of 10 times the amount thrown.", MonopolyCardEffect.AdvanceToNearestUtility, 0),

        // Money cards
        new("chance-bank-dividend", "Bank pays you dividend of $50.", MonopolyCardEffect.CollectFromBank, 50),
        new("chance-building-loan", "Your building and loan matures. Collect $150.", MonopolyCardEffect.CollectFromBank, 150),
        new("chance-poor-tax", "Pay poor tax of $15.", MonopolyCardEffect.PayToBank, 15),
        new("chance-chairman", "You have been elected Chairman of the Board. Pay each player $50.", MonopolyCardEffect.PayToPlayers, 50),

        // Special cards
        new("chance-go-to-jail", "Go to Jail. Go directly to Jail. Do not pass Go. Do not collect $200.", MonopolyCardEffect.GoToJail, 0),
        new("chance-get-out-of-jail", "Get out of Jail Free. This card may be kept until needed or sold.", MonopolyCardEffect.GetOutOfJailFree, 0),

        // Repairs card (deferred implementation - treated as $0 in base version)
        new("chance-repairs", "Make general repairs on all your property: For each house pay $25, for each hotel pay $100.", MonopolyCardEffect.PropertyRepairs, 25, 100)
    }.AsReadOnly();

    /// <summary>
    /// Gets the standard Community Chest deck (16 cards).
    /// </summary>
    public static IReadOnlyList<MonopolyCardDefinition> CommunityChestCards { get; } = new List<MonopolyCardDefinition>
    {
        // Movement cards
        new("cc-advance-go", "Advance to Go. Collect $200.", MonopolyCardEffect.AdvanceToPosition, 0),

        // Money cards - collect from bank
        new("cc-bank-error", "Bank error in your favor. Collect $200.", MonopolyCardEffect.CollectFromBank, 200),
        new("cc-doctor-fee", "Doctor's fee. Pay $50.", MonopolyCardEffect.PayToBank, 50),
        new("cc-stock-sale", "From sale of stock you get $50.", MonopolyCardEffect.CollectFromBank, 50),
        new("cc-grand-opera", "Grand Opera Night. Collect $50 from every player for opening night seats.", MonopolyCardEffect.CollectFromPlayers, 50),
        new("cc-holiday-fund", "Holiday fund matures. Receive $100.", MonopolyCardEffect.CollectFromBank, 100),
        new("cc-income-tax-refund", "Income tax refund. Collect $20.", MonopolyCardEffect.CollectFromBank, 20),
        new("cc-birthday", "It is your birthday. Collect $10 from every player.", MonopolyCardEffect.CollectFromPlayers, 10),
        new("cc-life-insurance", "Life insurance matures. Collect $100.", MonopolyCardEffect.CollectFromBank, 100),
        new("cc-hospital-fees", "Hospital fees. Pay $100.", MonopolyCardEffect.PayToBank, 100),
        new("cc-school-fees", "School fees. Pay $50.", MonopolyCardEffect.PayToBank, 50),
        new("cc-consultancy-fee", "Receive $25 consultancy fee.", MonopolyCardEffect.CollectFromBank, 25),
        new("cc-beauty-contest", "You have won second prize in a beauty contest. Collect $10.", MonopolyCardEffect.CollectFromBank, 10),
        new("cc-inherit", "You inherit $100.", MonopolyCardEffect.CollectFromBank, 100),

        // Special cards
        new("cc-go-to-jail", "Go to Jail. Go directly to Jail. Do not pass Go. Do not collect $200.", MonopolyCardEffect.GoToJail, 0),
        new("cc-get-out-of-jail", "Get out of Jail Free. This card may be kept until needed or sold.", MonopolyCardEffect.GetOutOfJailFree, 0)
    }.AsReadOnly();

    /// <summary>
    /// Deck identifier for Chance cards.
    /// </summary>
    public const string ChanceDeckId = "chance-deck";

    /// <summary>
    /// Deck identifier for Community Chest cards.
    /// </summary>
    public const string CommunityChestDeckId = "community-chest-deck";

    /// <summary>
    /// Pile identifier for draw pile.
    /// </summary>
    public const string DrawPileId = "draw";

    /// <summary>
    /// Pile identifier for discard pile.
    /// </summary>
    public const string DiscardPileId = "discard";
}
