namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Minimal game builder wiring scaffolding for deck-building core on top of the Cards module.
/// </summary>
public class DeckBuildingGameBuilder : GameBuilder
{
    /// <summary>
    /// Builds minimal topology and players to satisfy core invariants.
    /// </summary>
    protected override void Build()
    {
        BoardId = "deckbuilding-core";

        // Minimal board topology (no tiles needed by logic but required by core invariants)
        AddDirection("N");
        AddTile("db-tile-a");
        AddTile("db-tile-b");
        WithTile("db-tile-a").WithRelationTo("db-tile-b").InDirection("N");

        // Minimal players
        AddPlayer("P1");
        AddPlayer("P2");

        // Phases will be added alongside concrete rules in subsequent WS-17 commits.
    }
}