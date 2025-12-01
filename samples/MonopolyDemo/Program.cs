using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.Cards;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║            MONOPOLY DEMO - Deterministic Game                 ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

// Create a 2-player game for demonstration
var builder = new MonopolyGameBuilder(
    playerCount: 2,
    startingCash: 1500,
    playerNames: new[] { "Alice", "Bob" });

var progress = builder.Compile();
var game = progress.Engine.Game;

Console.WriteLine("=== Game Setup ===");
Console.WriteLine($"Board: {game.Board.Id}");
Console.WriteLine($"Players: {string.Join(", ", game.Players.Select(p => p.Id))}");
Console.WriteLine($"Squares: {game.Board.Tiles.Count()}");
Console.WriteLine();

// Initialize player states with starting cash
var initialPlayerStates = builder.CreateInitialPlayerStates(progress).ToList();
progress = progress.NewState(initialPlayerStates);

Console.WriteLine("=== Initial Player States ===");
PrintPlayerStates(progress.State, game);

// Pre-defined dice rolls for deterministic gameplay simulation
// Format: (die1, die2) -> total spaces to move
var diceRolls = new[]
{
    (3, 4),  // 7 - Alice lands on Chance 1
    (2, 3),  // 5 - Bob lands on Reading Railroad  
    (1, 2),  // 3 - Alice lands on Baltic Avenue
    (4, 3),  // 7 - Bob lands on Electric Company
    (6, 1),  // 7 - Alice lands on Jail (Just Visiting)
    (5, 5),  // 10 - Bob lands on Community Chest 2 (doubles!)
    (3, 3),  // 6 - Bob lands on Indiana Avenue (doubles again!)
    (2, 2),  // 4 - Bob lands on Water Works (third doubles = Go To Jail!)
    (4, 2),  // 6 - Alice lands on St. James Place
    (3, 1),  // 4 - Bob in jail, tries to roll doubles, fails
    (5, 1),  // 6 - Alice lands on New York Avenue
    (2, 4),  // 6 - Bob pays $50 fine, gets out of jail, lands on St. James Place
    (6, 6),  // 12 - Alice lands on Pacific Avenue (doubles!)
    (3, 2),  // 5 - Alice lands on Chance 3
    (1, 3),  // 4 - Bob lands on Indiana Avenue
    (4, 5),  // 9 - Alice lands on Go! Collects $200
};

int turnNumber = 0;
int rollIndex = 0;
const int maxTurns = 20;

Console.WriteLine("\n=== Game Play ===\n");

while (!IsGameEnded(progress.State) && turnNumber < maxTurns && rollIndex < diceRolls.Length)
{
    turnNumber++;

    var activePlayer = GetActivePlayer(progress.State, game);
    if (activePlayer is null)
    {
        Console.WriteLine("No active player found. Ending game.");
        break;
    }

    var playerState = progress.State.GetStates<MonopolyPlayerState>()
        .FirstOrDefault(ps => ps.Player.Equals(activePlayer));

    if (playerState is null)
    {
        Console.WriteLine($"Player state not found for {activePlayer.Id}. Ending game.");
        break;
    }

    Console.WriteLine($"┌─ Turn {turnNumber}: {activePlayer.Id}'s turn ─────────────────────────────");
    Console.WriteLine($"│  Cash: ${playerState.Cash}");

    if (playerState.InJail)
    {
        Console.WriteLine($"│  Status: IN JAIL (Turn {playerState.JailTurns + 1}/3)");
    }

    // Get current position
    var token = game.Artifacts.OfType<Piece>().FirstOrDefault(p => p.Owner?.Equals(activePlayer) == true);
    var tokenState = token is not null
        ? progress.State.GetStates<PieceState>().FirstOrDefault(ps => ps.Artifact.Equals(token))
        : null;

    int currentPosition = tokenState?.CurrentTile is not null
        ? MonopolyBoardConfiguration.GetPosition(tokenState.CurrentTile.Id)
        : 0;

    var currentSquare = MonopolyBoardConfiguration.GetSquare(currentPosition);
    Console.WriteLine($"│  Current: [{currentPosition}] {currentSquare.Name}");

    // Roll dice
    var (die1, die2) = diceRolls[rollIndex++];
    int diceTotal = die1 + die2;
    bool isDoubles = die1 == die2;

    Console.WriteLine($"│  Dice: {die1} + {die2} = {diceTotal}{(isDoubles ? " (DOUBLES!)" : "")}");

    // Handle jail - uses core events for jail mechanics
    if (playerState.InJail)
    {
        if (isDoubles)
        {
            Console.WriteLine("│  → Rolled doubles! Gets out of jail!");
            var releaseEvent = new GetOutOfJailGameEvent(activePlayer, GetOutOfJailMethod.RolledDoubles);
            progress = progress.HandleEvent(releaseEvent);
        }
        else if (playerState.JailTurns >= 2)
        {
            Console.WriteLine("│  → Third turn in jail. Must pay $50 fine.");
            var releaseEvent = new GetOutOfJailGameEvent(activePlayer, GetOutOfJailMethod.PaidFine);
            progress = progress.HandleEvent(releaseEvent);
        }
        else
        {
            Console.WriteLine("│  → Failed to roll doubles. Stays in jail.");
            // Use core event to increment jail turns
            var stayInJailEvent = new StayInJailGameEvent(activePlayer);
            progress = progress.HandleEvent(stayInJailEvent);

            // Use core event to switch to next player
            var endTurnEvent = new EndTurnGameEvent();
            progress = progress.HandleEvent(endTurnEvent);

            Console.WriteLine("└───────────────────────────────────────────────────────────────\n");
            continue;
        }
    }

    // Check for triple doubles -> Go to Jail
    // Note: The MovePlayerStateMutator now tracks consecutive doubles automatically,
    // but we still need to check BEFORE the move for the jail condition
    if (isDoubles && playerState.ConsecutiveDoubles >= 2)
    {
        Console.WriteLine("│  → THIRD CONSECUTIVE DOUBLES! Go To Jail!");
        var jailEvent = new GoToJailGameEvent(activePlayer);
        progress = progress.HandleEvent(jailEvent);

        // Use core event to switch to next player
        var endTurnEvent = new EndTurnGameEvent();
        progress = progress.HandleEvent(endTurnEvent);

        Console.WriteLine("└───────────────────────────────────────────────────────────────\n");
        continue;
    }

    // Move player - MovePlayerStateMutator now handles:
    // 1. Piece position update
    // 2. Consecutive doubles tracking
    // 3. Passing Go detection and $200 bonus
    var moveEvent = new MovePlayerGameEvent(activePlayer, die1, die2);
    progress = progress.HandleEvent(moveEvent);

    // Get updated position from state (set by mutator)
    var updatedTokenState = token is not null
        ? progress.State.GetStates<PieceState>().FirstOrDefault(ps => ps.Artifact.Equals(token))
        : null;
    int newPosition = updatedTokenState?.CurrentTile is not null
        ? MonopolyBoardConfiguration.GetPosition(updatedTokenState.CurrentTile.Id)
        : (currentPosition + diceTotal) % 40;

    var landedSquare = MonopolyBoardConfiguration.GetSquare(newPosition);
    Console.WriteLine($"│  → Moves to [{newPosition}] {landedSquare.Name}");

    // Check if passed Go (based on positions)
    bool passedGo = newPosition < currentPosition && currentPosition != 0;
    if (passedGo)
    {
        Console.WriteLine("│  → Passed GO! Collects $200 (handled by mutator)");
        // Note: The $200 is now automatically added by MovePlayerStateMutator
        playerState = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(activePlayer));
    }

    // Handle landing on square
    HandleLandingSquare(activePlayer, newPosition, landedSquare, diceTotal, ref progress, game);

    // Get updated player state after all actions
    playerState = progress.State.GetStates<MonopolyPlayerState>()
        .FirstOrDefault(ps => ps.Player.Equals(activePlayer));

    if (playerState is not null)
    {
        Console.WriteLine($"│  Cash after turn: ${playerState.Cash}");
    }

    // Check bankruptcy
    if (playerState is not null && playerState.Cash < 0)
    {
        Console.WriteLine($"│  → {activePlayer.Id} is BANKRUPT!");
        var eliminateEvent = new EliminatePlayerGameEvent(activePlayer, null);
        progress = progress.HandleEvent(eliminateEvent);
    }

    // Switch player if no doubles or turn ends - use core event
    if (!isDoubles)
    {
        var endTurnEvent = new EndTurnGameEvent();
        progress = progress.HandleEvent(endTurnEvent);
    }
    else
    {
        Console.WriteLine("│  → Rolled doubles! Gets another turn.");
    }

    Console.WriteLine("└───────────────────────────────────────────────────────────────\n");
}

Console.WriteLine("\n=== Game Summary ===");
PrintPlayerStates(progress.State, game);
PrintPropertyOwnership(progress.State);

if (IsGameEnded(progress.State))
{
    Console.WriteLine("\n*** GAME ENDED ***");
    var outcome = progress.State.GetStates<MonopolyOutcomeState>().FirstOrDefault();
    if (outcome is not null)
    {
        Console.WriteLine($"Winner: {outcome.Winner.Id}");
    }
}
else
{
    Console.WriteLine($"\n*** Demo ended after {turnNumber} turns ***");
}

// Demonstrate house building concept
Console.WriteLine("\n=== House Building Demo (Conceptual) ===\n");
DemonstrateHouseBuildingConcept();

Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                      Demo Complete!                            ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");

// House building demonstration - shows the mechanics without modifying actual game state
static void DemonstrateHouseBuildingConcept()
{
    Console.WriteLine("House/Hotel building requires a complete monopoly (all properties in a color group).");
    Console.WriteLine("\nExample: Brown Color Group (Mediterranean + Baltic)");
    Console.WriteLine("═══════════════════════════════════════════════════\n");

    // Create a simulated ownership state to demonstrate
    var ownership = new PropertyOwnershipState()
        .SetOwner(1, "Alice")  // Mediterranean Avenue
        .SetOwner(3, "Alice"); // Baltic Avenue

    var mediterranean = MonopolyBoardConfiguration.GetSquare(1);
    var baltic = MonopolyBoardConfiguration.GetSquare(3);

    Console.WriteLine($"Initial state:");
    Console.WriteLine($"  [{1}] {mediterranean.Name,-25} - Base rent: ${mediterranean.BaseRent}");
    Console.WriteLine($"  [{3}] {baltic.Name,-25} - Base rent: ${baltic.BaseRent}");
    Console.WriteLine($"\n  With monopoly (2× rent): ${mediterranean.BaseRent * 2} and ${baltic.BaseRent * 2}");
    Console.WriteLine($"  House cost per property: ${mediterranean.HouseCost}");

    Console.WriteLine("\nRent progression with houses:");
    Console.WriteLine("┌────────────────────────────┬──────────────────────────┐");
    Console.WriteLine("│ Mediterranean Avenue       │ Baltic Avenue            │");
    Console.WriteLine("├────────────────────────────┼──────────────────────────┤");

    for (int houses = 0; houses <= 5; houses++)
    {
        var medRent = mediterranean.GetRentForHouseCount(houses);
        var balRent = baltic.GetRentForHouseCount(houses);
        var label = houses switch
        {
            0 => "No houses (monopoly)",
            5 => "Hotel",
            _ => $"{houses} house(s)"
        };

        // Adjust for monopoly bonus at 0 houses
        if (houses == 0)
        {
            medRent = mediterranean.BaseRent * 2;
            balRent = baltic.BaseRent * 2;
        }

        Console.WriteLine($"│ {label,-17} ${medRent,-6} │ {label,-17} ${balRent,-4} │");
    }

    Console.WriteLine("└────────────────────────────┴──────────────────────────┘");

    // Show even building rule
    Console.WriteLine("\n=== Even Building Rule ===");
    Console.WriteLine("Houses must be built evenly across all properties in a color group.");
    Console.WriteLine("\nExample build sequence for Brown:");
    Console.WriteLine("  1. Build on Mediterranean (0→1 house)");
    Console.WriteLine("  2. Build on Baltic (0→1 house)  ← Must build here before second on Mediterranean");
    Console.WriteLine("  3. Build on Mediterranean (1→2 houses)");
    Console.WriteLine("  4. Build on Baltic (1→2 houses)");
    Console.WriteLine("  ... continue until all have 4 houses");
    Console.WriteLine("  9. Upgrade Mediterranean to hotel (4 houses → hotel)");
    Console.WriteLine("  10. Upgrade Baltic to hotel");

    // Demonstrate the building logic
    Console.WriteLine("\n=== Simulating House Building ===");
    var simOwnership = new PropertyOwnershipState()
        .SetOwner(1, "Alice")
        .SetOwner(3, "Alice");

    int totalCost = 0;
    for (int round = 1; round <= 5; round++)
    {
        var roundLabel = round < 5 ? $"Round {round}" : "Hotel round";
        Console.WriteLine($"\n{roundLabel}:");

        foreach (var pos in new[] { 1, 3 })
        {
            var square = MonopolyBoardConfiguration.GetSquare(pos);
            var currentHouses = simOwnership.GetHouseCount(pos);

            bool canBuild;
            if (currentHouses < 4)
            {
                canBuild = simOwnership.CanBuildHouse(pos, "Alice", PropertyColorGroup.Brown);
            }
            else
            {
                canBuild = simOwnership.CanBuildHotel(pos, "Alice", PropertyColorGroup.Brown);
            }

            if (canBuild)
            {
                simOwnership = simOwnership.AddHouse(pos);
                totalCost += square.HouseCost;
                var newHouses = simOwnership.GetHouseCount(pos);
                var rent = square.GetRentForHouseCount(newHouses);
                var improvement = newHouses == 5 ? "HOTEL" : $"{newHouses} house(s)";
                Console.WriteLine($"  ✓ [{pos}] {square.Name}: Built → {improvement}, Rent: ${rent}");
            }
            else
            {
                Console.WriteLine($"  ✗ [{pos}] {square.Name}: Cannot build (even building rule)");
            }
        }
    }

    Console.WriteLine($"\nTotal investment: ${totalCost} (${totalCost / 2} per property)");
    Console.WriteLine($"Final rent if someone lands on Mediterranean with hotel: ${mediterranean.GetRentForHouseCount(5)}");
    Console.WriteLine($"Final rent if someone lands on Baltic with hotel: ${baltic.GetRentForHouseCount(5)}");
}

// Helper methods
static bool IsGameEnded(GameState state)
{
    return state.GetStates<GameEndedState>().Any();
}

static Player? GetActivePlayer(GameState state, Game game)
{
    var activePlayerState = state.GetStates<ActivePlayerState>().FirstOrDefault(aps => aps.IsActive);
    return activePlayerState?.Artifact;
}

static void PrintPlayerStates(GameState state, Game game)
{
    Console.WriteLine("┌─ Player States ──────────────────────────────────────────────");
    foreach (var playerState in state.GetStates<MonopolyPlayerState>())
    {
        var status = playerState.IsBankrupt ? "BANKRUPT" :
                     playerState.InJail ? $"IN JAIL ({playerState.JailTurns + 1}/3)" :
                     "Active";

        var token = game.Artifacts.OfType<Piece>().FirstOrDefault(p => p.Owner?.Equals(playerState.Player) == true);
        var tokenState = token is not null
            ? state.GetStates<PieceState>().FirstOrDefault(ps => ps.Artifact.Equals(token))
            : null;

        int position = tokenState?.CurrentTile is not null
            ? MonopolyBoardConfiguration.GetPosition(tokenState.CurrentTile.Id)
            : 0;

        var square = MonopolyBoardConfiguration.GetSquare(position);

        Console.WriteLine($"│  {playerState.Player.Id,-10}: ${playerState.Cash,5} | {status,-15} | Position: [{position}] {square.Name}");
    }

    Console.WriteLine("└───────────────────────────────────────────────────────────────");
}

static void PrintPropertyOwnership(GameState state)
{
    var ownership = state.GetExtras<PropertyOwnershipState>();
    if (ownership is null)
    {
        return;
    }

    var ownedProperties = new List<(int Position, string Owner, int Houses)>();

    for (int i = 0; i < 40; i++)
    {
        var owner = ownership.GetOwner(i);
        if (owner is not null)
        {
            var houses = ownership.GetHouseCount(i);
            ownedProperties.Add((i, owner, houses));
        }
    }

    if (ownedProperties.Count > 0)
    {
        Console.WriteLine("┌─ Property Ownership ─────────────────────────────────────────");
        foreach (var (position, owner, houses) in ownedProperties)
        {
            var square = MonopolyBoardConfiguration.GetSquare(position);
            var houseStr = houses switch
            {
                0 => "",
                5 => " [HOTEL]",
                _ => $" [{houses}H]"
            };
            Console.WriteLine($"│  [{position,2}] {square.Name,-25} → {owner}{houseStr}");
        }

        Console.WriteLine("└───────────────────────────────────────────────────────────────");
    }
}

static void HandleLandingSquare(
    Player player,
    int position,
    MonopolySquareInfo square,
    int diceTotal,
    ref GameProgress progress,
    Game game)
{
    switch (square.SquareType)
    {
        case SquareType.Property:
        case SquareType.Railroad:
        case SquareType.Utility:
            HandlePropertySquare(player, position, square, diceTotal, ref progress);
            break;

        case SquareType.IncomeTax:
            Console.WriteLine("│  → Income Tax! Pays $200");
            var incomeTaxEvent = new PayTaxGameEvent(player, 200);
            progress = progress.HandleEvent(incomeTaxEvent);

            // Update player cash
            var playerStateIncome = progress.State.GetStates<MonopolyPlayerState>()
                .First(ps => ps.Player.Equals(player));
            var newPlayerStateIncome = playerStateIncome.AdjustCash(-200);
            progress = progress.NewState([newPlayerStateIncome]);
            break;

        case SquareType.LuxuryTax:
            Console.WriteLine("│  → Luxury Tax! Pays $75");
            var luxuryTaxEvent = new PayTaxGameEvent(player, 75);
            progress = progress.HandleEvent(luxuryTaxEvent);

            var playerStateLuxury = progress.State.GetStates<MonopolyPlayerState>()
                .First(ps => ps.Player.Equals(player));
            var newPlayerStateLuxury = playerStateLuxury.AdjustCash(-75);
            progress = progress.NewState([newPlayerStateLuxury]);
            break;

        case SquareType.GoToJail:
            Console.WriteLine("│  → GO TO JAIL!");
            var goToJailEvent = new GoToJailGameEvent(player);
            progress = progress.HandleEvent(goToJailEvent);

            // Move token to Jail (position 10)
            var token = game.Artifacts.OfType<Piece>().FirstOrDefault(p => p.Owner?.Equals(player) == true);
            if (token is not null)
            {
                var jailTile = game.Board.GetTile("square-10");
                if (jailTile is not null)
                {
                    var jailTokenState = new PieceState(token, jailTile);
                    progress = progress.NewState([jailTokenState]);
                }
            }

            // Update player jail state
            var playerStateJail = progress.State.GetStates<MonopolyPlayerState>()
                .First(ps => ps.Player.Equals(player));
            var jailedState = playerStateJail.GoToJail();
            progress = progress.NewState([jailedState]);
            break;

        case SquareType.Chance:
            HandleCardDraw(player, MonopolyCardDecks.ChanceDeckId, "Chance", ref progress);
            break;

        case SquareType.CommunityChest:
            HandleCardDraw(player, MonopolyCardDecks.CommunityChestDeckId, "Community Chest", ref progress);
            break;

        case SquareType.FreeParking:
            Console.WriteLine("│  → Free Parking. Nothing happens.");
            break;

        case SquareType.Jail:
            Console.WriteLine("│  → Just Visiting Jail.");
            break;

        case SquareType.Go:
            Console.WriteLine("│  → Landed on GO!");
            break;
    }
}

static void HandlePropertySquare(
    Player player,
    int position,
    MonopolySquareInfo square,
    int diceTotal,
    ref GameProgress progress)
{
    var ownership = progress.State.GetExtras<PropertyOwnershipState>();
    var owner = ownership?.GetOwner(position);
    var playerState = progress.State.GetStates<MonopolyPlayerState>()
        .First(ps => ps.Player.Equals(player));

    if (owner is null)
    {
        // Property is unowned - try to buy
        if (playerState.Cash >= square.Price)
        {
            Console.WriteLine($"│  → Buys {square.Name} for ${square.Price}");
            var buyEvent = new BuyPropertyGameEvent(player, position);
            progress = progress.HandleEvent(buyEvent);
        }
        else
        {
            Console.WriteLine($"│  → Cannot afford {square.Name} (${square.Price}) - auction deferred");
        }
    }
    else if (!string.Equals(owner, player.Id, StringComparison.Ordinal))
    {
        // Property owned by someone else - pay rent
        int rent = RentCalculator.CalculateRent(square, owner, ownership!, diceTotal);
        Console.WriteLine($"│  → Pays ${rent} rent to {owner}");
        var rentEvent = new PayRentGameEvent(player, position, diceTotal);
        progress = progress.HandleEvent(rentEvent);
    }
    else
    {
        Console.WriteLine($"│  → Owns this property.");
    }
}

static void HandleCardDraw(
    Player player,
    string deckId,
    string deckName,
    ref GameProgress progress)
{
    var cardsState = progress.State.GetExtras<MonopolyCardsState>();
    if (cardsState is null)
    {
        Console.WriteLine($"│  → {deckName}! (Cards state not found)");
        return;
    }

    var deck = cardsState.GetDeck(deckId);
    if (deck is null || deck.DrawPile.Count == 0)
    {
        Console.WriteLine($"│  → {deckName}! (Empty deck)");
        return;
    }

    // Get top card without actually drawing (to display)
    var topCard = deck.DrawPile[0];
    Console.WriteLine($"│  → {deckName}: \"{topCard.Text}\"");

    // Apply the card effect based on type
    var playerState = progress.State.GetStates<MonopolyPlayerState>()
        .First(ps => ps.Player.Equals(player));

    switch (topCard.Effect)
    {
        case MonopolyCardEffect.CollectFromBank:
            Console.WriteLine($"│     → Collects ${topCard.Value} from the bank");
            var collectState = playerState.AdjustCash(topCard.Value);
            progress = progress.NewState([collectState]);
            break;

        case MonopolyCardEffect.PayToBank:
            Console.WriteLine($"│     → Pays ${topCard.Value} to the bank");
            var payState = playerState.AdjustCash(-topCard.Value);
            progress = progress.NewState([payState]);
            break;

        case MonopolyCardEffect.GoToJail:
            Console.WriteLine("│     → Goes directly to Jail!");
            var jailState = playerState.GoToJail();
            progress = progress.NewState([jailState]);
            break;

        case MonopolyCardEffect.GetOutOfJailFree:
            Console.WriteLine("│     → Keeps Get Out of Jail Free card!");
            var jailCardState = playerState.WithGetOutOfJailCard(true);
            progress = progress.NewState([jailCardState]);
            break;

        case MonopolyCardEffect.CollectFromPlayers:
            var otherPlayers = progress.State.GetStates<MonopolyPlayerState>()
                .Where(ps => !ps.Player.Equals(player) && !ps.IsBankrupt)
                .ToList();
            int totalCollected = topCard.Value * otherPlayers.Count;
            Console.WriteLine($"│     → Collects ${topCard.Value} from each player (${totalCollected} total)");
            var updates = new List<IArtifactState>();
            foreach (var other in otherPlayers)
            {
                updates.Add(other.AdjustCash(-topCard.Value));
            }

            updates.Add(playerState.AdjustCash(totalCollected));
            progress = progress.NewState(updates);
            break;

        case MonopolyCardEffect.PayToPlayers:
            var recipients = progress.State.GetStates<MonopolyPlayerState>()
                .Where(ps => !ps.Player.Equals(player) && !ps.IsBankrupt)
                .ToList();
            int totalPaid = topCard.Value * recipients.Count;
            Console.WriteLine($"│     → Pays ${topCard.Value} to each player (${totalPaid} total)");
            var payUpdates = new List<IArtifactState>();
            foreach (var recipient in recipients)
            {
                payUpdates.Add(recipient.AdjustCash(topCard.Value));
            }

            payUpdates.Add(playerState.AdjustCash(-totalPaid));
            progress = progress.NewState(payUpdates);
            break;

        case MonopolyCardEffect.AdvanceToPosition:
            Console.WriteLine($"│     → Advances to position {topCard.Value} (movement deferred)");
            break;

        case MonopolyCardEffect.MoveBackward:
            Console.WriteLine($"│     → Moves back {topCard.Value} spaces (movement deferred)");
            break;

        case MonopolyCardEffect.AdvanceToNearestRailroad:
            Console.WriteLine("│     → Advances to nearest railroad (movement deferred)");
            break;

        case MonopolyCardEffect.AdvanceToNearestUtility:
            Console.WriteLine("│     → Advances to nearest utility (movement deferred)");
            break;

        case MonopolyCardEffect.PropertyRepairs:
            Console.WriteLine("│     → Property repairs (houses/hotels deferred)");
            break;

        default:
            Console.WriteLine($"│     → Effect {topCard.Effect} (not implemented)");
            break;
    }

    // Update deck state - move top card to discard
    // Note: In actual gameplay, this would be handled via the DrawCardGameEvent.
    // For the demo, we directly manipulate the state for simplicity.
    var (newDeckState, _) = deck.DrawCard();
    var newCardsState = cardsState.WithUpdatedDeck(newDeckState);
    // Use the DrawCardGameEvent to trigger proper state update
    var drawEvent = new DrawCardGameEvent(player, deckId);
    // Skip the event - the demo already applied the card effect manually above
    // The actual implementation would use: progress = progress.HandleEvent(drawEvent);
}
