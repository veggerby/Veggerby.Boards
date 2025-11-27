using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class RentCalculatorTests
{
    [Fact]
    public void CalculateRent_PropertyWithoutMonopoly_ReturnsBaseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue, base rent $2
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(2);
    }

    [Fact]
    public void CalculateRent_PropertyWithMonopoly_ReturnsDoubleBaseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue, base rent $2

        // Brown properties are at positions 1 and 3
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1");

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(4); // 2 * 2 = 4
    }

    [Fact]
    public void CalculateRent_OneRailroad_Returns25()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(5); // Reading Railroad
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player1");

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(25);
    }

    [Fact]
    public void CalculateRent_TwoRailroads_Returns50()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(5); // Reading Railroad
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player1")
            .SetOwner(15, "player1"); // Pennsylvania Railroad

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(50);
    }

    [Fact]
    public void CalculateRent_ThreeRailroads_Returns100()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(5); // Reading Railroad
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player1")
            .SetOwner(15, "player1")
            .SetOwner(25, "player1"); // B&O Railroad

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(100);
    }

    [Fact]
    public void CalculateRent_FourRailroads_Returns200()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(5); // Reading Railroad
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player1")
            .SetOwner(15, "player1")
            .SetOwner(25, "player1")
            .SetOwner(35, "player1"); // Short Line Railroad

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(200);
    }

    [Fact]
    public void CalculateRent_OneUtility_Returns4TimesDice()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(12); // Electric Company
        var ownership = new PropertyOwnershipState()
            .SetOwner(12, "player1");

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership, diceRoll: 7);

        // assert
        rent.Should().Be(28); // 4 * 7
    }

    [Fact]
    public void CalculateRent_TwoUtilities_Returns10TimesDice()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(12); // Electric Company
        var ownership = new PropertyOwnershipState()
            .SetOwner(12, "player1")
            .SetOwner(28, "player1"); // Water Works

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership, diceRoll: 7);

        // assert
        rent.Should().Be(70); // 10 * 7
    }
}
