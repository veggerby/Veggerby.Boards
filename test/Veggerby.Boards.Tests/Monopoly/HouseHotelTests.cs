using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class HouseHotelTests
{
    [Fact]
    public void GetHouseCount_NoHouses_ReturnsZero()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var count = state.GetHouseCount(1);

        // assert
        count.Should().Be(0);
    }

    [Fact]
    public void SetHouseCount_SetsHouses()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var newState = state.SetHouseCount(1, 3);

        // assert
        newState.GetHouseCount(1).Should().Be(3);
        newState.GetOwner(1).Should().Be("player1"); // Owner unchanged
    }

    [Fact]
    public void AddHouse_IncrementsHouseCount()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var newState = state.AddHouse(1);

        // assert
        newState.GetHouseCount(1).Should().Be(1);
    }

    [Fact]
    public void AddHouse_MultipleAdds_StackCorrectly()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var newState = state.AddHouse(1).AddHouse(1).AddHouse(1);

        // assert
        newState.GetHouseCount(1).Should().Be(3);
    }

    [Fact]
    public void HasHotel_FiveHouses_ReturnsTrue()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetHouseCount(1, 5);

        // act
        var hasHotel = state.HasHotel(1);

        // assert
        hasHotel.Should().BeTrue();
    }

    [Fact]
    public void HasHotel_FourHouses_ReturnsFalse()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetHouseCount(1, 4);

        // act
        var hasHotel = state.HasHotel(1);

        // assert
        hasHotel.Should().BeFalse();
    }

    [Fact]
    public void CanBuildHouse_WithMonopoly_ReturnsTrue()
    {
        // arrange
        // Brown properties are at positions 1 and 3
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1");

        // act
        var canBuild = state.CanBuildHouse(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeTrue();
    }

    [Fact]
    public void CanBuildHouse_WithoutMonopoly_ReturnsFalse()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var canBuild = state.CanBuildHouse(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeFalse();
    }

    [Fact]
    public void CanBuildHouse_EvenBuildingRule_BlocksUnevenBuilding()
    {
        // arrange
        // Brown properties at 1 and 3 - build one house on position 1
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 1);

        // act - try to build a second house on position 1 (position 3 has 0 houses)
        var canBuild = state.CanBuildHouse(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeFalse(); // Must build on position 3 first
    }

    [Fact]
    public void CanBuildHouse_EvenBuildingRule_AllowsEvenBuilding()
    {
        // arrange
        // Brown properties at 1 and 3 - both have 1 house
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 1)
            .SetHouseCount(3, 1);

        // act - try to build on position 1 (both have same house count)
        var canBuild = state.CanBuildHouse(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeTrue();
    }

    [Fact]
    public void CanBuildHouse_AtMaxHouses_ReturnsFalse()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 4)
            .SetHouseCount(3, 4);

        // act
        var canBuild = state.CanBuildHouse(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeFalse(); // Need hotel upgrade instead
    }

    [Fact]
    public void CanBuildHotel_WithFourHousesAllProperties_ReturnsTrue()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 4)
            .SetHouseCount(3, 4);

        // act
        var canBuild = state.CanBuildHotel(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeTrue();
    }

    [Fact]
    public void CanBuildHotel_NotAllPropertiesHaveFourHouses_ReturnsFalse()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 4)
            .SetHouseCount(3, 3);

        // act
        var canBuild = state.CanBuildHotel(1, "player1", PropertyColorGroup.Brown);

        // assert
        canBuild.Should().BeFalse();
    }

    [Fact]
    public void CanBuildHotel_OnRailroad_ReturnsFalse()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(5, "player1")
            .SetOwner(15, "player1")
            .SetOwner(25, "player1")
            .SetOwner(35, "player1");

        // act
        var canBuild = state.CanBuildHotel(5, "player1", PropertyColorGroup.Railroad);

        // assert
        canBuild.Should().BeFalse();
    }

    [Fact]
    public void GetMinHouseCountInColorGroup_ReturnsCorrectMin()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 3)
            .SetHouseCount(3, 1);

        // act
        var min = state.GetMinHouseCountInColorGroup("player1", PropertyColorGroup.Brown);

        // assert
        min.Should().Be(1);
    }

    [Fact]
    public void GetMaxHouseCountInColorGroup_ReturnsCorrectMax()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 3)
            .SetHouseCount(3, 1);

        // act
        var max = state.GetMaxHouseCountInColorGroup("player1", PropertyColorGroup.Brown);

        // assert
        max.Should().Be(3);
    }

    // Rent calculation tests with houses
    [Fact]
    public void CalculateRent_WithOneHouse_ReturnsOneHouseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 1);

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(10); // $10 with 1 house
    }

    [Fact]
    public void CalculateRent_WithTwoHouses_ReturnsTwoHouseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 2);

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(30); // $30 with 2 houses
    }

    [Fact]
    public void CalculateRent_WithThreeHouses_ReturnsThreeHouseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 3);

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(90); // $90 with 3 houses
    }

    [Fact]
    public void CalculateRent_WithFourHouses_ReturnsFourHouseRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 4);

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(160); // $160 with 4 houses
    }

    [Fact]
    public void CalculateRent_WithHotel_ReturnsHotelRent()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetHouseCount(1, 5); // Hotel

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(250); // $250 with hotel
    }

    [Fact]
    public void CalculateRent_BoardwalkWithHotel_Returns2000()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(39); // Boardwalk
        var ownership = new PropertyOwnershipState()
            .SetOwner(37, "player1") // Park Place
            .SetOwner(39, "player1") // Boardwalk
            .SetHouseCount(39, 5); // Hotel

        // act
        var rent = RentCalculator.CalculateRent(squareInfo, "player1", ownership);

        // assert
        rent.Should().Be(2000); // $2000 with hotel on Boardwalk
    }

    // Square info tests
    [Fact]
    public void MonopolySquareInfo_CanBuildHouses_TrueForProperty()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue

        // act & assert
        squareInfo.CanBuildHouses.Should().BeTrue();
    }

    [Fact]
    public void MonopolySquareInfo_CanBuildHouses_FalseForRailroad()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(5); // Reading Railroad

        // act & assert
        squareInfo.CanBuildHouses.Should().BeFalse();
    }

    [Fact]
    public void MonopolySquareInfo_CanBuildHouses_FalseForUtility()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(12); // Electric Company

        // act & assert
        squareInfo.CanBuildHouses.Should().BeFalse();
    }

    [Fact]
    public void MonopolySquareInfo_HouseCost_BrownProperties_50()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(1); // Mediterranean Avenue

        // act & assert
        squareInfo.HouseCost.Should().Be(50);
    }

    [Fact]
    public void MonopolySquareInfo_HouseCost_DarkBlueProperties_200()
    {
        // arrange
        var squareInfo = MonopolyBoardConfiguration.GetSquare(39); // Boardwalk

        // act & assert
        squareInfo.HouseCost.Should().Be(200);
    }

    [Fact]
    public void GetHouseCostForColorGroup_ReturnsCorrectCosts()
    {
        // arrange & act & assert
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Brown).Should().Be(50);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.LightBlue).Should().Be(50);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Pink).Should().Be(100);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Orange).Should().Be(100);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Red).Should().Be(150);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Yellow).Should().Be(150);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Green).Should().Be(200);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.DarkBlue).Should().Be(200);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Railroad).Should().Be(0);
        MonopolyBoardConfiguration.GetHouseCostForColorGroup(PropertyColorGroup.Utility).Should().Be(0);
    }
}
