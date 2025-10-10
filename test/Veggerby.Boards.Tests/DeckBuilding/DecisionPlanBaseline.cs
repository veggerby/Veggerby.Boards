namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Locked baseline of the deck-building DecisionPlan (ordered phase:event entries + signature).
/// Update only with intentional engine wiring changes; regenerate via temporary capture test.
/// </summary>
public static class DecisionPlanBaseline
{
    public static readonly string[] Entries = new[]
    {
        "db-setup:CreateDeckEvent",
        "db-setup:EndTurnSegmentEvent",
        "db-action:DrawWithReshuffleEvent",
        "db-action:TrashFromHandEvent",
        "db-action:EndTurnSegmentEvent",
        "db-buy:GainFromSupplyEvent",
        "db-buy:EndTurnSegmentEvent",
        "db-cleanup:CleanupToDiscardEvent",
        "db-cleanup:EndTurnSegmentEvent",
    };

    public const string Signature = "01B3E2805E9304679B7CE60A0D0F088CACB1219C8AA4A4EE979D37662A767429";
}