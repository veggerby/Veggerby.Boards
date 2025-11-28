using System.Linq;

using Veggerby.Boards.Risk;

namespace Veggerby.Boards.Tests.Risk;

/// <summary>
/// Tests for RiskGameBuilder and game initialization.
/// </summary>
public class RiskGameBuilderTests
{
    [Fact]
    public void Build_CreatesGameWith24Territories()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        var game = builder.Compile().Game;

        // assert
        game.Board.Tiles.Count().Should().Be(24);
    }

    [Fact]
    public void Build_Creates4Continents()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        _ = builder.Compile();

        // assert
        builder.Continents.Count.Should().Be(4);
    }

    [Fact]
    public void Build_ContinentsHaveCorrectTerritoryCount()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        _ = builder.Compile();

        // assert
        builder.Continents.Single(c => c.Id == RiskIds.Continents.North).TerritoryIds.Count.Should().Be(6);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.South).TerritoryIds.Count.Should().Be(4);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.Europe).TerritoryIds.Count.Should().Be(7);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.Asia).TerritoryIds.Count.Should().Be(7);
    }

    [Fact]
    public void Build_ContinentsHaveCorrectBonusArmies()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        _ = builder.Compile();

        // assert
        builder.Continents.Single(c => c.Id == RiskIds.Continents.North).BonusArmies.Should().Be(3);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.South).BonusArmies.Should().Be(2);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.Europe).BonusArmies.Should().Be(5);
        builder.Continents.Single(c => c.Id == RiskIds.Continents.Asia).BonusArmies.Should().Be(7);
    }

    [Fact]
    public void Build_CreatesAdjacentRelations()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        var game = builder.Compile().Game;

        // assert
        game.Board.TileRelations.Should().NotBeEmpty();

        // Verify some known adjacencies
        var alaskaNorthWest = game.Board.TileRelations.FirstOrDefault(
            r => (r.From.Id == RiskIds.Territories.Alaska && r.To.Id == RiskIds.Territories.NorthWest) ||
                 (r.From.Id == RiskIds.Territories.NorthWest && r.To.Id == RiskIds.Territories.Alaska));

        alaskaNorthWest.Should().NotBeNull();
    }

    [Fact]
    public void Build_CreatesCrossContinentConnections()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        var game = builder.Compile().Game;

        // assert
        // Alaska to Siberia (NA to Asia)
        var alaskaSiberia = game.Board.TileRelations.FirstOrDefault(
            r => (r.From.Id == RiskIds.Territories.Alaska && r.To.Id == RiskIds.Territories.Siberia) ||
                 (r.From.Id == RiskIds.Territories.Siberia && r.To.Id == RiskIds.Territories.Alaska));

        alaskaSiberia.Should().NotBeNull();

        // Greenland to Iceland (NA to Europe)
        var greenlandIceland = game.Board.TileRelations.FirstOrDefault(
            r => (r.From.Id == RiskIds.Territories.GreenLand && r.To.Id == RiskIds.Territories.Iceland) ||
                 (r.From.Id == RiskIds.Territories.Iceland && r.To.Id == RiskIds.Territories.GreenLand));

        greenlandIceland.Should().NotBeNull();
    }

    [Fact]
    public void Build_Creates2Players()
    {
        // arrange
        var builder = new RiskGameBuilder();

        // act
        var game = builder.Compile().Game;

        // assert
        game.Players.Count().Should().Be(2);
        game.Players.Should().Contain(p => p.Id == RiskIds.Players.Red);
        game.Players.Should().Contain(p => p.Id == RiskIds.Players.Blue);
    }
}
