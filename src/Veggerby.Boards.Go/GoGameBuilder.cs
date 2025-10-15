using Veggerby.Boards.Go.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Go;

/// <summary>
/// Configures a Go board (default 19x19) and scaffolds stone pools for both players plus pass/placement phase.
/// </summary>
/// <remarks>
/// Creates a new Go game builder.
/// </remarks>
/// <param name="size">Board dimension (default 19). 9 or 13 useful for testing.</param>
public sealed class GoGameBuilder(int size = 19) : GameBuilder
{
    private readonly int _size = size;

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
        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.West);

        for (int x = 1; x <= _size; x++)
        {
            for (int y = 1; y <= _size; y++)
            {
                var id = $"tile-{x}-{y}";
                var tile = AddTile(id);
                if (x > 1) { tile.WithRelationTo($"tile-{x - 1}-{y}").InDirection(Constants.Directions.West); }
                if (x < _size) { tile.WithRelationTo($"tile-{x + 1}-{y}").InDirection(Constants.Directions.East); }
                if (y > 1) { tile.WithRelationTo($"tile-{x}-{y - 1}").InDirection(Constants.Directions.North); }
                if (y < _size) { tile.WithRelationTo($"tile-{x}-{y + 1}").InDirection(Constants.Directions.South); }
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