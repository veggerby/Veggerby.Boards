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

    // Handle jail
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
            // Increment jail turns and pass turn
            var updatedPlayerState = playerState.IncrementJailTurns();
            progress = progress.NewState([updatedPlayerState]);

            // Switch to next player
            SwitchToNextPlayer(ref progress, game);

            Console.WriteLine("└───────────────────────────────────────────────────────────────\n");
            continue;
        }
    }

    // Check for triple doubles -> Go to Jail
    if (isDoubles)
    {
        var currentDoubles = playerState.ConsecutiveDoubles + 1;
        if (currentDoubles >= 3)
        {
            Console.WriteLine("│  → THIRD CONSECUTIVE DOUBLES! Go To Jail!");
            var jailEvent = new GoToJailGameEvent(activePlayer);
            progress = progress.HandleEvent(jailEvent);

            // Update player jail state
            var jailedPlayerState = progress.State.GetStates<MonopolyPlayerState>()
                .First(ps => ps.Player.Equals(activePlayer))
                .GoToJail();
            progress = progress.NewState([jailedPlayerState]);

            SwitchToNextPlayer(ref progress, game);

            Console.WriteLine("└───────────────────────────────────────────────────────────────\n");
            continue;
        }
        else
        {
            var updatedPlayerState = playerState.WithConsecutiveDoubles(currentDoubles);
            progress = progress.NewState([updatedPlayerState]);
            playerState = updatedPlayerState;
        }
    }
    else
    {
        // Reset consecutive doubles if not doubles
        if (playerState.ConsecutiveDoubles > 0)
        {
            var updatedPlayerState = playerState.WithConsecutiveDoubles(0);
            progress = progress.NewState([updatedPlayerState]);
            playerState = updatedPlayerState;
        }
    }

    // Move player
    int newPosition = (currentPosition + diceTotal) % 40;
    bool passedGo = newPosition < currentPosition && currentPosition != 0;

    var moveEvent = new MovePlayerGameEvent(activePlayer, die1, die2);
    progress = progress.HandleEvent(moveEvent);

    // Update token position manually (simulating movement)
    if (token is not null)
    {
        var newTileId = MonopolyBoardConfiguration.GetTileId(newPosition);
        var newTile = game.Board.GetTile(newTileId);
        if (newTile is not null)
        {
            var newTokenState = new PieceState(token, newTile);
            progress = progress.NewState([newTokenState]);
        }
    }

    var landedSquare = MonopolyBoardConfiguration.GetSquare(newPosition);
    Console.WriteLine($"│  → Moves to [{newPosition}] {landedSquare.Name}");

    // Handle passing Go
    if (passedGo)
    {
        Console.WriteLine("│  → Passed GO! Collects $200");
        var passGoEvent = new PassGoGameEvent(activePlayer);
        progress = progress.HandleEvent(passGoEvent);

        // Update player cash
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

    // Switch player if no doubles or turn ends
    if (!isDoubles)
    {
        SwitchToNextPlayer(ref progress, game);
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

Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                      Demo Complete!                            ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");

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

static void SwitchToNextPlayer(ref GameProgress progress, Game game)
{
    var players = game.Players.ToList();
    var currentActive = progress.State.GetStates<ActivePlayerState>().FirstOrDefault(aps => aps.IsActive);

    if (currentActive is null)
    {
        return;
    }

    int currentIndex = players.FindIndex(p => p.Equals(currentActive.Artifact));
    int nextIndex = (currentIndex + 1) % players.Count;

    var updates = new List<IArtifactState>();
    updates.Add(new ActivePlayerState(currentActive.Artifact, false));
    updates.Add(new ActivePlayerState(players[nextIndex], true));

    progress = progress.NewState(updates);
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

    var ownedProperties = new List<(int Position, string Owner)>();

    for (int i = 0; i < 40; i++)
    {
        var owner = ownership.GetOwner(i);
        if (owner is not null)
        {
            ownedProperties.Add((i, owner));
        }
    }

    if (ownedProperties.Count > 0)
    {
        Console.WriteLine("┌─ Property Ownership ─────────────────────────────────────────");
        foreach (var (position, owner) in ownedProperties)
        {
            var square = MonopolyBoardConfiguration.GetSquare(position);
            Console.WriteLine($"│  [{position,2}] {square.Name,-25} → {owner}");
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
