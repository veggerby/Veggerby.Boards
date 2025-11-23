using Veggerby.Boards;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Ludo.Conditions;
using Veggerby.Boards.Ludo.Events;
using Veggerby.Boards.Ludo.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Ludo;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining the Ludo/Parcheesi board, pieces, dice and phase / rule set.
/// </summary>
/// <remarks>
/// Demonstrates dice-driven race game with entry mechanics (roll 6 to enter), circular track topology,
/// home stretch completion, capture mechanics, and safe square immunity.
/// </remarks>
public class LudoGameBuilder : GameBuilder
{
    private readonly int _playerCount;
    private readonly bool _bonusTurnOnSix;

    /// <summary>
    /// Initializes a new instance of the <see cref="LudoGameBuilder"/> class.
    /// </summary>
    /// <param name="playerCount">Number of players (2-4). Default is 4.</param>
    /// <param name="bonusTurnOnSix">Whether rolling a 6 grants a bonus turn. Default is true.</param>
    public LudoGameBuilder(int playerCount = 4, bool bonusTurnOnSix = true)
    {
        if (playerCount < 2 || playerCount > 4)
        {
            throw new System.ArgumentOutOfRangeException(nameof(playerCount), "Player count must be between 2 and 4");
        }

        _playerCount = playerCount;
        _bonusTurnOnSix = bonusTurnOnSix;
    }

    /// <summary>
    /// Configures the Ludo game artifacts, initial state and rule/phase flow.
    /// </summary>
    protected override void Build()
    {
        BoardId = "ludo";

        // Define players
        var playerColors = new[] { "red", "blue", "green", "yellow" };
        for (int i = 0; i < _playerCount; i++)
        {
            AddPlayer(playerColors[i]);
        }

        // Define directions
        AddDirection("forward");
        AddDirection("home-entry");

        // Create main circular track (52 squares)
        for (int i = 0; i < 52; i++)
        {
            AddTile($"track-{i}");
        }

        // Connect track tiles in circular fashion
        for (int i = 0; i < 52; i++)
        {
            WithTile($"track-{i}")
                .WithRelationTo($"track-{(i + 1) % 52}").InDirection("forward");
        }

        // Create home base tiles and home stretch for each player
        for (int p = 0; p < _playerCount; p++)
        {
            var playerColor = playerColors[p];

            // Home base (starting area where pieces wait to enter)
            AddTile($"base-{playerColor}");

            // Home stretch (5 tiles per player)
            for (int h = 0; h < 5; h++)
            {
                AddTile($"home-{playerColor}-{h}");
            }

            // Connect home stretch tiles
            for (int h = 0; h < 4; h++)
            {
                WithTile($"home-{playerColor}-{h}")
                    .WithRelationTo($"home-{playerColor}-{h + 1}").InDirection("forward");
            }

            // Entry point from main track to home stretch
            // Each player's home entry is at their 13th square starting from their start position
            int entrySquare = p * 13;
            WithTile($"track-{entrySquare}")
                .WithRelationTo($"home-{playerColor}-0").InDirection("home-entry");
        }

        // Define safe squares (starting positions)
        var safeSquares = new System.Collections.Generic.List<string>();
        for (int p = 0; p < _playerCount; p++)
        {
            safeSquares.Add($"track-{p * 13}"); // Each player's starting square
        }

        // Store safe squares in state
        WithState(new LudoSafeSquaresState(safeSquares));

        // Create 4 pieces per player
        for (int p = 0; p < _playerCount; p++)
        {
            var playerColor = playerColors[p];
            for (int i = 0; i < 4; i++)
            {
                AddPiece($"{playerColor}-{i + 1}")
                    .WithOwner(playerColor)
                    .HasDirection("forward")
                    .CanRepeat();
            }
        }

        // Add dice
        AddDice("dice").HasNoValue();

        // Initial state: all pieces in base
        for (int p = 0; p < _playerCount; p++)
        {
            var playerColor = playerColors[p];
            for (int i = 0; i < 4; i++)
            {
                WithPiece($"{playerColor}-{i + 1}").OnTile($"base-{playerColor}");
            }
        }

        // Game phases
        AddGamePhase("ludo gameplay")
            .WithEndGameDetection(
                game => new AllPiecesHomeCondition(_playerCount),
                game => new LudoEndGameMutator())
            .If<SingleActivePlayerGameStateCondition>()
                .And<GameNotEndedCondition>()
            .Then()
                .ForEvent<RollDiceGameEvent<int>>()
                    .If(game => new DiceGameEventCondition<int>([game.GetArtifact<Artifacts.Dice>("dice")!]))
                    .Then()
                        .Do<DiceStateMutator<int>>()
                .ForEvent<EnterPieceGameEvent>()
                    .If<EnterPieceCondition>()
                    .Then()
                        .Do<EnterPieceStateMutator>()
                        .Do(game => new GenericClearDiceStateMutator([game.GetArtifact<Artifacts.Dice>("dice")!]))
                        .Do(game =>
                        {
                            if (_bonusTurnOnSix)
                            {
                                return new ConditionalBonusTurnStateMutator(game.GetArtifact<Artifacts.Dice>("dice")!);
                            }
                            return new NextPlayerStateMutator(new NullGameStateCondition());
                        })
                .ForEvent<MovePieceGameEvent>()
                    .If<PieceIsActivePlayerGameEventCondition>()
                        .And(game => new HasDiceValueGameEventCondition([game.GetArtifact<Artifacts.Dice>("dice")!]))
                        .And<PieceNotInBaseCondition>()
                        .And<ExactFinishCondition>()
                    .Then()
                        .Do<LudoCapturePieceStateMutator>()
                        .Do<MovePieceStateMutator>()
                        .Do(game => new ClearDiceStateMutator([game.GetArtifact<Artifacts.Dice>("dice")!]))
                        .Do(game =>
                        {
                            if (_bonusTurnOnSix)
                            {
                                return new ConditionalBonusTurnStateMutator(game.GetArtifact<Artifacts.Dice>("dice")!);
                            }
                            return new NextPlayerStateMutator(new NullGameStateCondition());
                        });
    }
}
