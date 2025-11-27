using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Risk;

namespace Veggerby.Boards.Tests.Risk;

/// <summary>
/// Tests for TerritoryState.
/// </summary>
public class TerritoryStateTests
{
    private readonly Tile _territory;
    private readonly Player _player1;
    private readonly Player _player2;

    public TerritoryStateTests()
    {
        _territory = new Tile("territory-1");
        _player1 = new Player("player-1");
        _player2 = new Player("player-2");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesState()
    {
        // arrange & act
        var state = new TerritoryState(_territory, _player1, 5);

        // assert
        state.Territory.Should().Be(_territory);
        state.Owner.Should().Be(_player1);
        state.ArmyCount.Should().Be(5);
        state.Artifact.Should().Be(_territory);
    }

    [Fact]
    public void Constructor_WithZeroArmies_ThrowsException()
    {
        // arrange & act & assert
        var act = () => new TerritoryState(_territory, _player1, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithNegativeArmies_ThrowsException()
    {
        // arrange & act & assert
        var act = () => new TerritoryState(_territory, _player1, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithArmyDelta_PositiveDelta_IncreasesArmies()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act
        var newState = state.WithArmyDelta(3);

        // assert
        newState.ArmyCount.Should().Be(8);
        newState.Owner.Should().Be(_player1);
        newState.Territory.Should().Be(_territory);
    }

    [Fact]
    public void WithArmyDelta_NegativeDelta_DecreasesArmies()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act
        var newState = state.WithArmyDelta(-2);

        // assert
        newState.ArmyCount.Should().Be(3);
    }

    [Fact]
    public void WithArmyDelta_DecreaseToOne_Succeeds()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act
        var newState = state.WithArmyDelta(-4);

        // assert
        newState.ArmyCount.Should().Be(1);
    }

    [Fact]
    public void WithArmyDelta_DecreaseBelowOne_ThrowsException()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act & assert
        var act = () => state.WithArmyDelta(-5);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WithNewOwner_ChangesOwnership()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act
        var newState = state.WithNewOwner(_player2, 3);

        // assert
        newState.Territory.Should().Be(_territory);
        newState.Owner.Should().Be(_player2);
        newState.ArmyCount.Should().Be(3);
    }

    [Fact]
    public void WithNewOwner_WithZeroArmies_ThrowsException()
    {
        // arrange
        var state = new TerritoryState(_territory, _player1, 5);

        // act & assert
        var act = () => state.WithNewOwner(_player2, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // arrange
        var state1 = new TerritoryState(_territory, _player1, 5);
        var state2 = new TerritoryState(_territory, _player1, 5);

        // act & assert
        state1.Equals(state2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentArmyCount_ReturnsFalse()
    {
        // arrange
        var state1 = new TerritoryState(_territory, _player1, 5);
        var state2 = new TerritoryState(_territory, _player1, 6);

        // act & assert
        state1.Equals(state2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentOwner_ReturnsFalse()
    {
        // arrange
        var state1 = new TerritoryState(_territory, _player1, 5);
        var state2 = new TerritoryState(_territory, _player2, 5);

        // act & assert
        state1.Equals(state2).Should().BeFalse();
    }
}
