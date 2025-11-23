using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Ludo;
using Veggerby.Boards.Ludo.Events;
using Veggerby.Boards.States;

Console.WriteLine("=== Ludo Demo - Deterministic Game ===\n");

// Create a 2-player game for faster demonstration
var builder = new LudoGameBuilder(playerCount: 2, bonusTurnOnSix: true);

// Compile the game with a deterministic dice sequence
var progress = builder.Compile();
var game = progress.Game;

Console.WriteLine($"Game: {game.Board.Id}");
Console.WriteLine($"Players: {string.Join(", ", game.Players.Select(p => p.Id))}");
Console.WriteLine($"Board tiles: {game.Board.Tiles.Count()}");
Console.WriteLine();

// Pre-defined dice rolls for a deterministic game
// This will demonstrate: entry, movement, capture, safe squares, and winning
var diceRolls = new[]
{
    6,  // Red enters piece 1
    3,  // Red moves piece 1 forward 3
    6,  // Blue enters piece 1
    2,  // Blue moves piece 1 forward 2
    6,  // Red enters piece 2
    4,  // Red moves piece 1 forward 4
    5,  // Blue moves piece 1 forward 5
    6,  // Red enters piece 3
    6,  // Red enters piece 4 (all pieces on board)
    3,  // Red moves piece 1 forward 3
    4,  // Blue moves piece 1 forward 4
    6,  // Red moves piece 1 forward 6
    3,  // Red moves piece 2 forward 3
    5,  // Blue moves piece 1 forward 5
    6,  // Red moves piece 1 forward 6
    2,  // Red moves piece 1 forward 2
};

var dice = game.GetArtifact<Dice>("dice");
int rollIndex = 0;
int turnNumber = 0;
const int maxTurns = 100; // Safety limit

Console.WriteLine("Starting game...\n");

while (!IsGameEnded(progress.State) && turnNumber < maxTurns)
{
    turnNumber++;
    var activePlayer = GetActivePlayer(progress.State, game);

    if (activePlayer == null)
    {
        Console.WriteLine("No active player found. Ending game.");
        break;
    }

    Console.WriteLine($"Turn {turnNumber}: {activePlayer.Id}'s turn");

    // Roll dice
    if (rollIndex >= diceRolls.Length)
    {
        Console.WriteLine("Ran out of pre-defined dice rolls. Ending demo.");
        break;
    }

    int rollValue = diceRolls[rollIndex++];
    var rollEvent = new RollDiceGameEvent<int>(new DiceState<int>(dice!, rollValue));
    progress = progress.HandleEvent(rollEvent);

    Console.WriteLine($"  Rolled: {rollValue}");

    // Try to enter a piece if we rolled 6 and have pieces in base
    if (rollValue == 6)
    {
        var baseTileId = $"base-{activePlayer.Id}";
        var piecesInBase = progress.State.GetStates<PieceState>()
            .Where(ps => ps.Artifact.Owner?.Equals(activePlayer) == true &&
                         ps.CurrentTile?.Id == baseTileId)
            .ToList();

        if (piecesInBase.Any())
        {
            var pieceToEnter = piecesInBase.First().Artifact as Piece;
            if (pieceToEnter != null)
            {
                var enterEvent = new EnterPieceGameEvent(pieceToEnter);
                var result = progress.HandleEvent(enterEvent);

                if (result.State != progress.State) // Event was successful
                {
                    progress = result;
                    Console.WriteLine($"  Entered piece: {pieceToEnter.Id}");
                    PrintGameState(progress.State, game);
                    continue;
                }
            }
        }
    }

    // Try to move a piece
    var movablePieces = progress.State.GetStates<PieceState>()
        .Where(ps => ps.Artifact.Owner?.Equals(activePlayer) == true &&
                     ps.CurrentTile != null &&
                     !ps.CurrentTile.Id.StartsWith($"base-"))
        .ToList();

    bool moveMade = false;
    foreach (var pieceState in movablePieces)
    {
        var piece = pieceState.Artifact as Piece;
        if (piece == null || pieceState.CurrentTile == null)
        {
            continue;
        }

        // Try to find a valid destination tile at the rolled distance
        // For Ludo, we need to traverse forward along the path
        Tile? currentTile = pieceState.CurrentTile;
        Tile? destinationTile = null;

        // Follow the "forward" direction for the dice value number of steps
        for (int step = 0; step < rollValue && currentTile != null; step++)
        {
            // Check for home entry first
            var homeEntryRelations = game.Board.TileRelations
                .Where(r => r.From.Equals(currentTile) && r.Direction.Id == "home-entry")
                .ToList();

            if (homeEntryRelations.Any())
            {
                // Entering home stretch
                currentTile = homeEntryRelations.First().To;
            }
            else
            {
                // Normal forward movement
                var nextRelations = game.Board.TileRelations
                    .Where(r => r.From.Equals(currentTile) && r.Direction.Id == "forward")
                    .ToList();

                if (nextRelations.Any())
                {
                    currentTile = nextRelations.First().To;
                }
                else
                {
                    currentTile = null;
                    break;
                }
            }
        }

        if (currentTile != null)
        {
            destinationTile = currentTile;

            // Try to resolve a path
            var resolver = new ResolveTilePathPatternVisitor(game.Board, pieceState.CurrentTile, destinationTile);
            foreach (var pattern in piece.Patterns)
            {
                pattern.Accept(resolver);
            }

            if (resolver.ResultPath != null)
            {
                var moveEvent = new MovePieceGameEvent(piece, resolver.ResultPath);
                var result = progress.HandleEvent(moveEvent);

                if (result.State != progress.State) // Event was successful
                {
                    progress = result;
                    Console.WriteLine($"  Moved piece: {piece.Id} from {pieceState.CurrentTile.Id} to {destinationTile.Id}");
                    moveMade = true;
                    break;
                }
            }
        }
    }

    if (!moveMade && movablePieces.Any())
    {
        Console.WriteLine($"  No valid move available");
    }

    PrintGameState(progress.State, game);
}

if (IsGameEnded(progress.State))
{
    Console.WriteLine("\n=== GAME ENDED ===");
    var outcome = progress.State.GetStates<LudoOutcomeState>().FirstOrDefault();
    if (outcome != null)
    {
        Console.WriteLine($"Winner: {outcome.Winner.Id}");
        Console.WriteLine($"Terminal condition: {outcome.TerminalCondition}");
    }
}
else
{
    Console.WriteLine($"\n=== Demo ended after {turnNumber} turns ===");
}

Console.WriteLine("\nDemo complete!");

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

static void PrintGameState(GameState state, Game game)
{
    // Print piece positions for all players
    var playerColors = new[] { "red", "blue" };

    foreach (var color in playerColors)
    {
        var player = game.GetPlayer(color);
        if (player == null)
        {
            continue;
        }

        var pieces = state.GetStates<PieceState>()
            .Where(ps => ps.Artifact.Owner?.Equals(player) == true)
            .ToList();

        var positions = pieces
            .Select(ps => $"{ps.Artifact.Id}@{ps.CurrentTile?.Id ?? "nowhere"}")
            .ToArray();

        Console.WriteLine($"  {color}: {string.Join(", ", positions)}");
    }

    Console.WriteLine();
}
