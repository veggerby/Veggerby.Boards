using Veggerby.Boards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Go.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Go;

/// <summary>
/// Configures a Go board (default 19x19) and scaffolds stone pools for both players plus pass/placement phase.
/// </summary>
public sealed class GoGameBuilder : GameBuilder
{
    private readonly int _size;

    /// <summary>
    /// Creates a new Go game builder.
    /// </summary>
    /// <param name="size">Board dimension (default 19). 9 or 13 useful for testing.</param>
    public GoGameBuilder(int size = 19)
    {
        _size = size;
    }

    /// <summary>
    /// Builds the Go game definition: sets up players, orthogonally connected board tiles, pre-allocates stone pieces,
    /// initializes Go-specific extras state and registers the play phase with supported events and mutators.
    /// </summary>
    /// <remarks>
    /// This override is invoked exactly once by the <see cref="GameBuilder"/> pipeline when compiling the game. It is
    /// deterministic; given the same size parameter it will always yield the same artifact graph and initial state.
    /// </remarks>
    protected override void Build()
    {
        BoardId = $"go-{_size}x{_size}";

        AddPlayer("black");
        AddPlayer("white");

        // Four orthogonal directions (liberty topology)
        AddDirection("north");
        AddDirection("east");
        AddDirection("south");
        AddDirection("west");

        for (int x = 1; x <= _size; x++)
        {
            for (int y = 1; y <= _size; y++)
            {
                var id = $"tile-{x}-{y}";
                var tile = AddTile(id);
                if (x > 1) { tile.WithRelationTo($"tile-{x - 1}-{y}").InDirection("west"); }
                if (x < _size) { tile.WithRelationTo($"tile-{x + 1}-{y}").InDirection("east"); }
                if (y > 1) { tile.WithRelationTo($"tile-{x}-{y - 1}").InDirection("north"); }
                if (y < _size) { tile.WithRelationTo($"tile-{x}-{y + 1}").InDirection("south"); }
            }
        }

        // Pre-create a pool of stones (conservatively size^2 for each color; only subset will be used)
        for (int i = 1; i <= _size * _size; i++)
        {
            AddPiece($"black-stone-{i}").WithOwner("black");
            AddPiece($"white-stone-{i}").WithOwner("white");
        }

        // Initial extras (board size persisted)
        WithState(new GoStateExtras(null, 0, _size));

        AddGamePhase("play")
            .If<InitialGameStateCondition>()
            .Then()
                .All()
                .ForEvent<PlaceStoneGameEvent>()
                    .Then()
                        .Do<PlaceStoneStateMutator>()
                .ForEvent<PassTurnGameEvent>()
                    .Then()
                        .Do<PassTurnStateMutator>();
    }
}