namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Locked baseline of the deck-building DecisionPlan (ordered phase:event entries + signature).
/// Update only with intentional engine wiring changes; regenerate via temporary capture test.
/// </summary>
public static class DecisionPlanBaseline
{
    public static readonly string[] Entries = new[]
    {
        "db-setup:RegisterCardDefinitionEvent",
        "db-setup:CreateDeckEvent",
        "db-setup:EndTurnSegmentEvent",
        "db-action:DrawWithReshuffleEvent",
        "db-action:TrashFromHandEvent",
        "db-action:EndTurnSegmentEvent",
        "db-buy:GainFromSupplyEvent",
        "db-buy:EndTurnSegmentEvent",
        "db-cleanup:CleanupToDiscardEvent",
        "db-cleanup:ComputeScoresEvent",
        "db-cleanup:EndGameEvent",
        "db-cleanup:EndTurnSegmentEvent",
    };
    // SHA-256 of the ordered phase:event entries above (captured via temporary baseline capture test)
    public const string Signature = "1FE3B652CCAFBC84834E243155E20FF992606BFB30530DBABE878349726B6414";
}