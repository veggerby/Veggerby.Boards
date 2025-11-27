using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Risk;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Risk;

/// <summary>
/// Tests for ReinforcementCalculator.
/// </summary>
public class ReinforcementCalculatorTests
{
    private readonly Player _player1;
    private readonly Player _player2;
    private readonly List<Continent> _continents;

    public ReinforcementCalculatorTests()
    {
        _player1 = new Player("player-1");
        _player2 = new Player("player-2");

        // Create continents
        _continents = new List<Continent>
        {
            new Continent("continent-a", "Continent A", 3, new[] { "territory-1", "territory-2", "territory-3" }),
            new Continent("continent-b", "Continent B", 2, new[] { "territory-4", "territory-5" }),
            new Continent("continent-c", "Continent C", 5, new[] { "territory-6", "territory-7", "territory-8", "territory-9", "territory-10", "territory-11", "territory-12" })
        };
    }

    [Fact]
    public void Calculate_WithMinimumTerritories_ReturnsMinimum3()
    {
        // arrange
        var tiles = new List<Tile>
        {
            new Tile("territory-1"),
            new Tile("territory-2"),
            new Tile("territory-3")
        };

        var territoryStates = tiles.Select(t => new TerritoryState(t, _player1, 1));
        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.Calculate(_player1, state, _continents);

        // assert
        result.Should().Be(6); // 3 territories = 3/3 = 1, but minimum is 3 + continent bonus of 3
    }

    [Fact]
    public void Calculate_With10Territories_Returns3Armies()
    {
        // arrange
        var tiles = new List<Tile>();

        // Use territory IDs that don't match continent definitions to avoid bonus
        for (int i = 100; i < 110; i++)
        {
            tiles.Add(new Tile($"territory-{i}"));
        }

        var territoryStates = tiles.Select(t => new TerritoryState(t, _player1, 1));
        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.Calculate(_player1, state, _continents);

        // assert
        // 10/3 = 3.33, floor = 3, which equals minimum
        result.Should().Be(3);
    }

    [Fact]
    public void Calculate_With12Territories_Returns4Armies()
    {
        // arrange
        var tiles = new List<Tile>();

        // Use territory IDs that don't match continent definitions to avoid bonus
        for (int i = 100; i < 112; i++)
        {
            tiles.Add(new Tile($"territory-{i}"));
        }

        var territoryStates = tiles.Select(t => new TerritoryState(t, _player1, 1));
        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.Calculate(_player1, state, _continents);

        // assert
        // 12/3 = 4, no continent bonus (territory IDs don't match continent)
        result.Should().Be(4);
    }

    [Fact]
    public void Calculate_WithContinentControl_IncludesBonusArmies()
    {
        // arrange
        var tiles = new List<Tile>
        {
            new Tile("territory-1"),
            new Tile("territory-2"),
            new Tile("territory-3") // Complete continent-a
        };

        var territoryStates = tiles.Select(t => new TerritoryState(t, _player1, 1));
        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.Calculate(_player1, state, _continents);

        // assert
        // Base: max(3, 3/3) = 3, Continent bonus: 3
        result.Should().Be(6);
    }

    [Fact]
    public void Calculate_WithMultipleContinentsControlled_SumsAllBonuses()
    {
        // arrange
        var tiles = new List<Tile>
        {
            new Tile("territory-1"),
            new Tile("territory-2"),
            new Tile("territory-3"), // Complete continent-a (bonus 3)
            new Tile("territory-4"),
            new Tile("territory-5")  // Complete continent-b (bonus 2)
        };

        var territoryStates = tiles.Select(t => new TerritoryState(t, _player1, 1));
        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.Calculate(_player1, state, _continents);

        // assert
        // Base: max(3, 5/3) = 3, Continent bonus: 3 + 2 = 5
        result.Should().Be(8);
    }

    [Fact]
    public void ControlsContinent_WhenAllOwned_ReturnsTrue()
    {
        // arrange
        var ownedIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "territory-1",
            "territory-2",
            "territory-3"
        };

        var continent = _continents[0]; // continent-a

        // act
        var result = ReinforcementCalculator.ControlsContinent(_player1, ownedIds, continent);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ControlsContinent_WhenPartiallyOwned_ReturnsFalse()
    {
        // arrange
        var ownedIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "territory-1",
            "territory-2"
            // Missing territory-3
        };

        var continent = _continents[0]; // continent-a

        // act
        var result = ReinforcementCalculator.ControlsContinent(_player1, ownedIds, continent);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CountOwnedTerritories_ReturnsCorrectCount()
    {
        // arrange
        var tiles = new List<Tile>
        {
            new Tile("territory-1"),
            new Tile("territory-2"),
            new Tile("territory-3"),
            new Tile("territory-4"),
            new Tile("territory-5")
        };

        var territoryStates = new List<IArtifactState>
        {
            new TerritoryState(tiles[0], _player1, 1),
            new TerritoryState(tiles[1], _player1, 1),
            new TerritoryState(tiles[2], _player2, 1),
            new TerritoryState(tiles[3], _player1, 1),
            new TerritoryState(tiles[4], _player2, 1)
        };

        var state = GameState.New(territoryStates);

        // act
        var result = ReinforcementCalculator.CountOwnedTerritories(_player1, state);

        // assert
        result.Should().Be(3);
    }
}
