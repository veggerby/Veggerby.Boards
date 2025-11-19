using Veggerby.Boards.Cards;
using Veggerby.Boards.States;

namespace CardGameDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Card Game Demo ===");
        Console.WriteLine("Demonstrating Veggerby.Boards Cards module");
        Console.WriteLine();

        // Setup with deterministic seed
        var builder = new CardsGameBuilder();
        builder.WithSeed(42UL);
        var progress = builder.Compile();

        Console.WriteLine("1. Initializing deck...");
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = builder.CreateInitialDeckEvent().Deck;
        PrintPileState(progress, deck!, "After initialization");

        // Shuffle
        Console.WriteLine("\n2. Shuffling deck...");
        var shuffleEvt = new ShuffleDeckEvent(deck!, CardsGameBuilder.Piles.Draw);
        progress = progress.HandleEvent(shuffleEvt);
        var state = progress.State.GetState<DeckState>(deck!);
        var shuffledOrder = state!.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id).ToList();
        Console.WriteLine($"   Shuffled order: {string.Join(", ", shuffledOrder)}");
        PrintPileState(progress, deck!, "After shuffle");

        // Play 3 rounds
        for (int round = 1; round <= 3; round++)
        {
            Console.WriteLine($"\n=== Round {round} ===");
            progress = PlayRound(progress, deck!, round);
        }

        Console.WriteLine("\n=== Demo Complete ===");
        Console.WriteLine("\nKey Takeaways:");
        Console.WriteLine("- All operations are deterministic (same seed = same results)");
        Console.WriteLine("- Peek doesn't change state (useful for previewing)");
        Console.WriteLine("- Reshuffle enables continuous play by recycling discard");
        Console.WriteLine("- State is immutable - each event creates new snapshot");
    }

    static GameProgress PlayRound(GameProgress progress, Deck deck, int roundNumber)
    {
        var state = progress.State.GetState<DeckState>(deck);

        // Check if we need to reshuffle before drawing
        if (state!.Piles[CardsGameBuilder.Piles.Draw].Count < 2 &&
            state.Piles[CardsGameBuilder.Piles.Discard].Count > 0)
        {
            Console.WriteLine("   Draw pile low - reshuffling discard into draw");
            var reshuffleEvt = new ReshuffleEvent(deck, CardsGameBuilder.Piles.Discard, CardsGameBuilder.Piles.Draw);
            progress = progress.HandleEvent(reshuffleEvt);
            PrintPileState(progress, deck, "After reshuffle");
        }

        // Deal 2 cards to hand
        Console.WriteLine("   Dealing 2 cards to hand...");
        state = progress.State.GetState<DeckState>(deck);
        var drawCount = Math.Min(2, state!.Piles[CardsGameBuilder.Piles.Draw].Count);
        if (drawCount > 0)
        {
            var drawEvt = new DrawCardsEvent(deck, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, drawCount);
            progress = progress.HandleEvent(drawEvt);
        }
        PrintPileState(progress, deck, "After deal");

        // Peek at top cards (no state change)
        state = progress.State.GetState<DeckState>(deck);
        var peekCount = Math.Min(3, state!.Piles[CardsGameBuilder.Piles.Draw].Count);
        if (peekCount > 0)
        {
            Console.WriteLine($"   Peeking at top {peekCount} cards...");
            var peekEvt = new PeekCardsEvent(deck, CardsGameBuilder.Piles.Draw, peekCount);
            progress = progress.HandleEvent(peekEvt);

            state = progress.State.GetState<DeckState>(deck);
            var topCards = state!.Piles[CardsGameBuilder.Piles.Draw].Take(peekCount).Select(c => c.Id).ToList();
            Console.WriteLine($"   Top cards: {string.Join(", ", topCards)}");
            PrintPileState(progress, deck, "After peek (state unchanged)");
        }

        // Draw one more card (hit)
        state = progress.State.GetState<DeckState>(deck);
        if (state!.Piles[CardsGameBuilder.Piles.Draw].Count > 0 && roundNumber == 1)
        {
            Console.WriteLine("   Drawing 1 more card (hit)...");
            var hitEvt = new DrawCardsEvent(deck, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 1);
            progress = progress.HandleEvent(hitEvt);
            PrintPileState(progress, deck, "After hit");
        }

        // Reveal hand
        state = progress.State.GetState<DeckState>(deck);
        var handCards = state!.Piles[CardsGameBuilder.Piles.Hand].ToList();
        if (handCards.Count > 0)
        {
            Console.WriteLine($"   Revealing hand: {string.Join(", ", handCards.Select(c => c.Id))}");
            var revealEvt = new RevealCardsEvent(deck, CardsGameBuilder.Piles.Hand, handCards);
            progress = progress.HandleEvent(revealEvt);
        }

        // Discard hand
        state = progress.State.GetState<DeckState>(deck);
        handCards = state!.Piles[CardsGameBuilder.Piles.Hand].ToList();
        if (handCards.Count > 0)
        {
            Console.WriteLine("   Discarding hand...");
            var discardEvt = new DiscardCardsEvent(deck, CardsGameBuilder.Piles.Discard, handCards);
            progress = progress.HandleEvent(discardEvt);
            PrintPileState(progress, deck, "After discard");
        }

        return progress;
    }

    static void PrintPileState(GameProgress progress, Deck deck, string label)
    {
        var state = progress.State.GetState<DeckState>(deck);
        if (state != null)
        {
            var draw = state.Piles[CardsGameBuilder.Piles.Draw].Count;
            var discard = state.Piles[CardsGameBuilder.Piles.Discard].Count;
            var hand = state.Piles[CardsGameBuilder.Piles.Hand].Count;
            var inplay = state.Piles[CardsGameBuilder.Piles.InPlay].Count;

            Console.WriteLine($"   {label}: Draw={draw}, Discard={discard}, Hand={hand}, InPlay={inplay}");
        }
    }
}
