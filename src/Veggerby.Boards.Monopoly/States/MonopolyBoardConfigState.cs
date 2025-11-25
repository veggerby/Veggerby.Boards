using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for Monopoly board configuration state.
/// </summary>
internal sealed class MonopolyBoardConfigMarker : Artifact
{
    public static readonly MonopolyBoardConfigMarker Instance = new();

    private MonopolyBoardConfigMarker() : base("monopoly-board-config") { }
}

/// <summary>
/// Immutable state containing board configuration data accessible at runtime.
/// </summary>
public sealed class MonopolyBoardConfigState : IArtifactState
{
    private readonly IReadOnlyList<MonopolySquareInfo> _squares;

    /// <inheritdoc />
    public Artifact Artifact => MonopolyBoardConfigMarker.Instance;

    /// <summary>
    /// Initializes a new instance using the standard board configuration.
    /// </summary>
    public MonopolyBoardConfigState()
    {
        _squares = MonopolyBoardConfiguration.Squares;
    }

    /// <summary>
    /// Gets the square info at a position.
    /// </summary>
    public MonopolySquareInfo GetSquare(int position)
    {
        if (position < 0 || position >= _squares.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return _squares[position];
    }

    /// <summary>
    /// Gets all squares.
    /// </summary>
    public IReadOnlyList<MonopolySquareInfo> Squares => _squares;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is MonopolyBoardConfigState;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MonopolyBoardConfigState;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return typeof(MonopolyBoardConfigState).GetHashCode();
    }
}
