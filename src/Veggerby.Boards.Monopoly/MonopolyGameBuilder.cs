using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Cards;
using Veggerby.Boards.Monopoly.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.Mutators;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining the Monopoly board, pieces, dice, and phase/rule set.
/// </summary>
/// <remarks>
/// Demonstrates economic state management with property ownership, rent collection,
/// cash flow, jail mechanics, and player elimination.
/// </remarks>
public class MonopolyGameBuilder : GameBuilder
{
    private readonly int _playerCount;
    private readonly int _startingCash;
    private readonly string[] _playerNames;
    private readonly int _cardShuffleSeed;

    /// <summary>
    /// Default starting cash amount.
    /// </summary>
    public const int DefaultStartingCash = 1500;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyGameBuilder"/> class.
    /// </summary>
    /// <param name="playerCount">Number of players (2-8). Default is 4.</param>
    /// <param name="startingCash">Starting cash per player. Default is $1500.</param>
    /// <param name="playerNames">Custom player names (optional).</param>
    /// <param name="cardShuffleSeed">Seed for deterministic card shuffling. Default is 42.</param>
    public MonopolyGameBuilder(int playerCount = 4, int startingCash = DefaultStartingCash, string[]? playerNames = null, int cardShuffleSeed = 42)
    {
        if (playerCount < 2 || playerCount > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCount), "Player count must be between 2 and 8");
        }

        if (startingCash < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingCash), "Starting cash must be non-negative");
        }

        _playerCount = playerCount;
        _startingCash = startingCash;
        _playerNames = playerNames ?? GetDefaultPlayerNames(playerCount);
        _cardShuffleSeed = cardShuffleSeed;

        if (_playerNames.Length != playerCount)
        {
            throw new ArgumentException($"Player names count ({_playerNames.Length}) must match player count ({playerCount})", nameof(playerNames));
        }
    }

    private static string[] GetDefaultPlayerNames(int count)
    {
        var names = new string[count];

        for (int i = 0; i < count; i++)
        {
            names[i] = $"Player-{i + 1}";
        }

        return names;
    }

    /// <summary>
    /// Configures the Monopoly game artifacts, initial state, and rule/phase flow.
    /// </summary>
    protected override void Build()
    {
        BoardId = "monopoly";

        // Define direction for circular track
        AddDirection("clockwise");

        // Create 40 board squares
        for (int i = 0; i < 40; i++)
        {
            AddTile(MonopolyBoardConfiguration.GetTileId(i));
        }

        // Connect squares in circular fashion
        for (int i = 0; i < 40; i++)
        {
            WithTile(MonopolyBoardConfiguration.GetTileId(i))
                .WithRelationTo(MonopolyBoardConfiguration.GetTileId((i + 1) % 40))
                .InDirection("clockwise");
        }

        // Define players
        for (int i = 0; i < _playerCount; i++)
        {
            AddPlayer(_playerNames[i]);
        }

        // Create one piece per player (token)
        for (int i = 0; i < _playerCount; i++)
        {
            AddPiece($"token-{_playerNames[i]}")
                .WithOwner(_playerNames[i])
                .HasDirection("clockwise")
                .CanRepeat();
        }

        // Add two dice
        AddDice("dice-1").HasNoValue();
        AddDice("dice-2").HasNoValue();

        // Set first player as active
        WithActivePlayer(_playerNames[0], true);
        for (int i = 1; i < _playerCount; i++)
        {
            WithActivePlayer(_playerNames[i], false);
        }

        // Initial state: all pieces on Go (position 0)
        var goTileId = MonopolyBoardConfiguration.GetTileId(0);
        for (int i = 0; i < _playerCount; i++)
        {
            WithPiece($"token-{_playerNames[i]}").OnTile(goTileId);
        }

        // Register board configuration state
        WithState(new MonopolyBoardConfigState());

        // Register empty property ownership state
        WithState(new PropertyOwnershipState());

        // Register card deck states
        var chanceDeck = new MonopolyCardDeckState(
            MonopolyCardDecks.ChanceDeckId,
            ShuffleCards(MonopolyCardDecks.ChanceCards, _cardShuffleSeed));

        var communityChestDeck = new MonopolyCardDeckState(
            MonopolyCardDecks.CommunityChestDeckId,
            ShuffleCards(MonopolyCardDecks.CommunityChestCards, _cardShuffleSeed + 1));

        WithState(new MonopolyCardsState(chanceDeck, communityChestDeck));

        // Game phases with endgame detection
        AddGamePhase("monopoly gameplay")
            .WithEndGameDetection(
                game => new MonopolyGameEndedCondition(),
                game => new MonopolyEndGameMutator())
            .If<SingleActivePlayerGameStateCondition>()
                .And<MonopolyGameNotEndedCondition>()
            .Then()
                // Dice rolling
                .ForEvent<RollDiceGameEvent<int>>()
                    .If(game => new DiceGameEventCondition<int>([
                        game.GetArtifact<Artifacts.Dice>("dice-1")!,
                        game.GetArtifact<Artifacts.Dice>("dice-2")!
                    ]))
                    .Then()
                        .Do<DiceStateMutator<int>>()
                // Movement
                .ForEvent<MovePlayerGameEvent>()
                    .If(game => new AlwaysValidCondition<MovePlayerGameEvent>())
                    .Then()
                        .Do<MovePlayerStateMutator>()
                // Passing Go
                .ForEvent<PassGoGameEvent>()
                    .If(game => new AlwaysValidCondition<PassGoGameEvent>())
                    .Then()
                        .Do<PassGoStateMutator>()
                // Buying property
                .ForEvent<BuyPropertyGameEvent>()
                    .If<CanBuyPropertyCondition>()
                    .Then()
                        .Do<BuyPropertyStateMutator>()
                // Paying rent
                .ForEvent<PayRentGameEvent>()
                    .If<MustPayRentCondition>()
                    .Then()
                        .Do<PayRentStateMutator>()
                // Paying tax
                .ForEvent<PayTaxGameEvent>()
                    .If(game => new AlwaysValidCondition<PayTaxGameEvent>())
                    .Then()
                        .Do<PayTaxStateMutator>()
                // Go to jail
                .ForEvent<GoToJailGameEvent>()
                    .If(game => new AlwaysValidCondition<GoToJailGameEvent>())
                    .Then()
                        .Do<GoToJailStateMutator>()
                // Get out of jail
                .ForEvent<GetOutOfJailGameEvent>()
                    .If<CanGetOutOfJailCondition>()
                    .Then()
                        .Do<GetOutOfJailStateMutator>()
                // Player elimination
                .ForEvent<EliminatePlayerGameEvent>()
                    .If(game => new AlwaysValidCondition<EliminatePlayerGameEvent>())
                    .Then()
                        .Do<EliminatePlayerStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new NullGameStateCondition()))
                // Draw card
                .ForEvent<DrawCardGameEvent>()
                    .If<CanDrawCardCondition>()
                    .Then()
                        .Do(game => new DrawCardStateMutator(_cardShuffleSeed));
    }

    private static List<MonopolyCardDefinition> ShuffleCards(IReadOnlyList<MonopolyCardDefinition> cards, int seed)
    {
        var list = cards.ToList();

        // Deterministic seeded shuffle using linear congruential generator
        // This avoids using System.Random which is banned
        uint state = (uint)seed;
        for (int i = list.Count - 1; i > 0; i--)
        {
            // Linear congruential generator step
            state = state * 1664525u + 1013904223u;
            int j = (int)(state % (uint)(i + 1));
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Creates initial player states with starting cash.
    /// </summary>
    /// <remarks>
    /// This should be called after Compile() to get initial player state events.
    /// </remarks>
    public IEnumerable<MonopolyPlayerState> CreateInitialPlayerStates(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        foreach (var player in progress.Engine.Game.Players)
        {
            yield return new MonopolyPlayerState(player, _startingCash);
        }
    }
}
