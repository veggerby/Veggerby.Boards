using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Risk;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                   RISK DEMO - Territory Conquest                  ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝\n");

// Build the Risk game with deterministic seed
var builder = new RiskGameBuilder();
builder.WithSeed(42); // Seed for determinism
var progress = builder.Compile();
var game = progress.Game;

Console.WriteLine("=== Game Setup ===");
Console.WriteLine($"Board: {game.Board.Id}");
Console.WriteLine($"Players: {string.Join(", ", game.Players.Select(p => p.Id))}");
Console.WriteLine($"Territories: {game.Board.Tiles.Count()}");
Console.WriteLine($"Continents: {builder.Continents.Count}");
Console.WriteLine();

// Display continent information
Console.WriteLine("=== Continents & Bonuses ===");
foreach (var continent in builder.Continents)
{
    Console.WriteLine($"  {continent.Name}: {continent.TerritoryIds.Count} territories, +{continent.BonusArmies} bonus armies");
}

Console.WriteLine();

// Get player references
var redPlayer = game.GetPlayer(RiskIds.Players.Red)!;
var bluePlayer = game.GetPlayer(RiskIds.Players.Blue)!;

// Initialize territory ownership with a balanced split
Console.WriteLine("=== Initial Territory Distribution ===\n");

// Set up initial territory ownership: alternating ownership
var territories = game.Board.Tiles.ToList();
var territoryStates = new List<IArtifactState>();
var isRed = true;

foreach (var territory in territories)
{
    var owner = isRed ? redPlayer : bluePlayer;
    var armies = 3; // Start with 3 armies each
    territoryStates.Add(new TerritoryState(territory, owner, armies));
    isRed = !isRed;
}

// Update the game state with initial territory distribution
progress = progress.NewState(territoryStates);

PrintTerritoryStatus(progress.State, game, builder.Continents);

Console.WriteLine("\n=== Starting Game Demo ===\n");

// Demo: Complete turn sequence for Red player
Console.WriteLine("┌─ RED's Turn ──────────────────────────────────────────────────────\n");

// Phase 1: Reinforcement
Console.WriteLine("│ PHASE 1: REINFORCEMENT");
Console.WriteLine("│ ────────────────────────────────────────────────────────────────");

// Get reinforcements from state (set by builder or calculated at turn start)
var riskExtras = progress.State.GetExtras<RiskStateExtras>();
var reinforcements = riskExtras?.ReinforcementsRemaining ?? 3;

// After state setup, recalculate based on current state
var redTerritoryCount = progress.State.GetStates<TerritoryState>()
    .Count(ts => ts.Owner.Equals(redPlayer));
var calculatedReinforcements = ReinforcementCalculator.Calculate(redPlayer, progress.State, builder.Continents);

Console.WriteLine($"│ Territories owned: {redTerritoryCount}");
Console.WriteLine($"│ Base reinforcements: {Math.Max(3, redTerritoryCount / 3)}");
Console.WriteLine($"│ Continent bonuses: {calculatedReinforcements - Math.Max(3, redTerritoryCount / 3)}");
Console.WriteLine($"│ Reinforcements available: {reinforcements}");
Console.WriteLine("│");

// Place reinforcements on Alaska
var alaska = game.Board.GetTile(RiskIds.Territories.Alaska)!;
var alaskaState = progress.State.GetState<TerritoryState>(alaska)!;

if (alaskaState.Owner.Equals(redPlayer) && reinforcements > 0)
{
    Console.WriteLine($"│ Placing {reinforcements} armies on Alaska...");
    var placeEvent = new PlaceArmiesGameEvent(redPlayer, alaska, reinforcements);
    progress = progress.HandleEvent(placeEvent);

    var newAlaskaState = progress.State.GetState<TerritoryState>(alaska)!;
    Console.WriteLine($"│ Alaska now has {newAlaskaState.ArmyCount} armies");
}

Console.WriteLine("│");

// Phase 2: Attack
Console.WriteLine("│ PHASE 2: ATTACK");
Console.WriteLine("│ ────────────────────────────────────────────────────────────────");

// Find an enemy territory adjacent to Alaska (NorthWest is adjacent and BLUE)
var northWestTile = game.Board.GetTile(RiskIds.Territories.NorthWest)!;
var northWestState = progress.State.GetState<TerritoryState>(northWestTile);

if (northWestState != null && !northWestState.Owner.Equals(redPlayer))
{
    Console.WriteLine($"│ Red attacks NorthWest Territory from Alaska!");
    Console.WriteLine($"│ Attacker armies: {progress.State.GetState<TerritoryState>(alaska)!.ArmyCount}");
    Console.WriteLine($"│ Defender armies: {northWestState.ArmyCount}");

    // Perform multiple attacks until conquest or retreat
    var attackCount = 0;
    var maxAttacks = 5;

    while (attackCount < maxAttacks)
    {
        attackCount++;
        var currentAlaska = progress.State.GetState<TerritoryState>(alaska)!;
        var currentNorthWest = progress.State.GetState<TerritoryState>(northWestTile);

        // Check if we've conquered it (ConqueredTerritoryState marker)
        var conqueredState = progress.State.GetStates<ConqueredTerritoryState>()
            .FirstOrDefault(cs => cs.Territory.Equals(northWestTile));

        if (conqueredState != null)
        {
            Console.WriteLine("│");
            Console.WriteLine("│ ★ CONQUEST! NorthWest Territory has been conquered!");

            // Move armies into conquered territory (must leave at least 1 in source)
            var sourceArmies = progress.State.GetState<TerritoryState>(alaska)!.ArmyCount;
            const int minRequired = 2; // Minimum per Risk rules (attacker dice)
            var armiesToMove = Math.Max(minRequired, Math.Min(sourceArmies - 1, 3)); // Move up to 3 but leave at least 1
            Console.WriteLine($"│ Moving {armiesToMove} armies into NorthWest (leaving 1 in Alaska)...");

            var conquerEvent = new ConquerTerritoryGameEvent(northWestTile, redPlayer, armiesToMove, alaska);
            progress = progress.HandleEvent(conquerEvent);

            var newNorthWestState = progress.State.GetState<TerritoryState>(northWestTile)!;
            Console.WriteLine($"│ NorthWest now owned by {newNorthWestState.Owner.Id} with {newNorthWestState.ArmyCount} armies");
            break;
        }

        // Need at least 2 armies to attack (1 must stay behind)
        if (currentAlaska.ArmyCount < 2)
        {
            Console.WriteLine("│ Not enough armies to continue attacking!");
            break;
        }

        // Determine attacker dice (max 3, but need to leave 1 army behind)
        var attackerDice = Math.Min(3, currentAlaska.ArmyCount - 1);

        Console.WriteLine($"│");
        Console.WriteLine($"│ Attack #{attackCount}:");
        Console.WriteLine($"│   Alaska: {currentAlaska.ArmyCount} armies ({attackerDice} dice)");
        Console.WriteLine($"│   NorthWest: {currentNorthWest?.ArmyCount ?? 0} armies");

        // Execute attack
        var attackEvent = new AttackGameEvent(alaska, northWestTile, attackerDice);
        var previousState = progress.State;
        progress = progress.HandleEvent(attackEvent);

        // Show results if attack happened
        if (!ReferenceEquals(progress.State, previousState))
        {
            // Get updated state to show combat result
            var resultAlaska = progress.State.GetState<TerritoryState>(alaska)!;
            var resultNorthWest = progress.State.GetState<TerritoryState>(northWestTile);
            var conqueredAfter = progress.State.GetStates<ConqueredTerritoryState>()
                .FirstOrDefault(cs => cs.Territory.Equals(northWestTile));

            var attackerLosses = currentAlaska.ArmyCount - resultAlaska.ArmyCount;

            // Calculate defender losses: previous army count minus current (0 if conquered)
            var previousDefenderArmies = currentNorthWest?.ArmyCount ?? 0;
            var currentDefenderArmies = conqueredAfter != null ? 0 : (resultNorthWest?.ArmyCount ?? 0);
            var defenderLosses = previousDefenderArmies - currentDefenderArmies;

            Console.WriteLine($"│   → Attacker lost: {attackerLosses}");
            Console.WriteLine($"│   → Defender lost: {defenderLosses}");
        }
    }
}
else
{
    Console.WriteLine("│ No valid attack target adjacent to Alaska");
}

Console.WriteLine("│");

// End attack phase
Console.WriteLine("│ Ending attack phase...");
var endAttackEvent = new EndAttackPhaseGameEvent();
progress = progress.HandleEvent(endAttackEvent);

Console.WriteLine("│");

// Phase 3: Fortify
Console.WriteLine("│ PHASE 3: FORTIFY");
Console.WriteLine("│ ────────────────────────────────────────────────────────────────");

// Try to fortify from Ontario to Alberta if both owned by Red
var ontarioTile = game.Board.GetTile(RiskIds.Territories.Ontario);
var albertaTile = game.Board.GetTile(RiskIds.Territories.Alberta);

if (ontarioTile != null && albertaTile != null)
{
    var ontarioForFortify = progress.State.GetState<TerritoryState>(ontarioTile);
    var albertaForFortify = progress.State.GetState<TerritoryState>(albertaTile);

    if (ontarioForFortify?.Owner.Equals(redPlayer) == true &&
        albertaForFortify?.Owner.Equals(redPlayer) == true &&
        ontarioForFortify.ArmyCount > 1)
    {
        var armiesToMove = Math.Min(2, ontarioForFortify.ArmyCount - 1);
        Console.WriteLine($"│ Fortifying: Moving {armiesToMove} armies from Ontario to Alberta");

        var fortifyEvent = new FortifyGameEvent(ontarioTile, albertaTile, armiesToMove);
        var previousFortifyState = progress.State;
        progress = progress.HandleEvent(fortifyEvent);

        if (!ReferenceEquals(progress.State, previousFortifyState))
        {
            var newOntario = progress.State.GetState<TerritoryState>(ontarioTile)!;
            var newAlberta = progress.State.GetState<TerritoryState>(albertaTile)!;
            Console.WriteLine($"│ Ontario: {newOntario.ArmyCount} armies");
            Console.WriteLine($"│ Alberta: {newAlberta.ArmyCount} armies");
        }
        else
        {
            Console.WriteLine("│ Fortification not allowed (territories might not be connected)");
        }
    }
    else
    {
        Console.WriteLine("│ Skipping fortification (conditions not met)");
    }
}

// End turn
Console.WriteLine("│");
Console.WriteLine("│ Ending turn...");
var endFortifyEvent = new EndFortifyPhaseGameEvent();
progress = progress.HandleEvent(endFortifyEvent);

Console.WriteLine("└───────────────────────────────────────────────────────────────────\n");

// Show final state
Console.WriteLine("=== Final Territory Status ===\n");
PrintTerritoryStatus(progress.State, game, builder.Continents);

// Check for game end
if (progress.State.GetStates<GameEndedState>().Any())
{
    var outcome = progress.State.GetStates<RiskOutcomeState>().FirstOrDefault();
    if (outcome != null)
    {
        Console.WriteLine("\n★★★ GAME OVER ★★★");
        Console.WriteLine($"Winner: {outcome.Winner.Id}");
        Console.WriteLine($"Condition: {outcome.TerminalCondition}");
    }
}

Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                         Demo Complete!                            ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");

// Helper methods
static void PrintTerritoryStatus(GameState state, Game game, IReadOnlyList<Continent> continents)
{
    foreach (var continent in continents)
    {
        Console.WriteLine($"┌─ {continent.Name} (bonus: +{continent.BonusArmies}) ────────────────────────────");

        foreach (var territoryId in continent.TerritoryIds)
        {
            var tile = game.Board.GetTile(territoryId);

            if (tile == null)
            {
                continue;
            }

            var territoryState = state.GetState<TerritoryState>(tile);

            if (territoryState != null)
            {
                var shortName = territoryId.Replace("territory-", "");
                var ownerName = territoryState.Owner.Id.Replace("player-", "").ToUpperInvariant();
                Console.WriteLine($"│  {shortName,-15} [{ownerName,4}] {territoryState.ArmyCount,2} armies");
            }
        }

        Console.WriteLine("└───────────────────────────────────────────────────────────────────\n");
    }

    // Summary
    var redCount = state.GetStates<TerritoryState>().Count(ts => ts.Owner.Id == RiskIds.Players.Red);
    var blueCount = state.GetStates<TerritoryState>().Count(ts => ts.Owner.Id == RiskIds.Players.Blue);
    var totalTerritories = state.GetStates<TerritoryState>().Count();

    Console.WriteLine($"Territory Control: RED = {redCount}/{totalTerritories}, BLUE = {blueCount}/{totalTerritories}");
}
