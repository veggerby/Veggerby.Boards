using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class MortgageTests
{
    [Fact]
    public void IsMortgaged_NewOwnership_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        // act
        var result = ownership.IsMortgaged(1);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mortgage_UnmortgagedProperty_SetsMortgagedTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        // act
        var result = ownership.Mortgage(1);

        // assert
        result.IsMortgaged(1).Should().BeTrue();
        result.GetOwner(1).Should().Be("player-1");
    }

    [Fact]
    public void Unmortgage_MortgagedProperty_SetsMortgagedFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        // act
        var result = ownership.Unmortgage(1);

        // assert
        result.IsMortgaged(1).Should().BeFalse();
        result.GetOwner(1).Should().Be("player-1");
    }

    [Fact]
    public void CanMortgage_UnownedProperty_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState();

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMortgage_OwnedByDifferentPlayer_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-2");

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMortgage_AlreadyMortgaged_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMortgage_PropertyWithHouses_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")  // Mediterranean Avenue
            .SetOwner(3, "player-1")  // Baltic Avenue (complete Brown monopoly)
            .SetHouseCount(1, 1);

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMortgage_OtherPropertyInGroupHasHouses_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")  // Mediterranean Avenue
            .SetOwner(3, "player-1")  // Baltic Avenue (complete Brown monopoly)
            .SetHouseCount(3, 1);     // House on Baltic

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMortgage_ValidProperty_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        // act
        var result = ownership.CanMortgage(1, "player-1");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanMortgage_Railroad_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player-1");  // Reading Railroad

        // act
        var result = ownership.CanMortgage(5, "player-1");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanMortgage_Utility_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(12, "player-1");  // Electric Company

        // act
        var result = ownership.CanMortgage(12, "player-1");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUnmortgage_NotMortgaged_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        // act
        var result = ownership.CanUnmortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUnmortgage_MortgagedProperty_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        // act
        var result = ownership.CanUnmortgage(1, "player-1");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUnmortgage_DifferentOwner_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-2")
            .Mortgage(1);

        // act
        var result = ownership.CanUnmortgage(1, "player-1");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasActiveMonopoly_AllUnmortgaged_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")   // Mediterranean Avenue
            .SetOwner(3, "player-1");  // Baltic Avenue

        // act
        var result = ownership.HasActiveMonopoly("player-1", PropertyColorGroup.Brown);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasActiveMonopoly_OneMortgaged_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")   // Mediterranean Avenue
            .SetOwner(3, "player-1")   // Baltic Avenue
            .Mortgage(1);              // Mortgage Mediterranean

        // act
        var result = ownership.HasActiveMonopoly("player-1", PropertyColorGroup.Brown);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBuildHouse_PropertyMortgaged_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")   // Mediterranean Avenue
            .SetOwner(3, "player-1")   // Baltic Avenue
            .Mortgage(1);              // Mortgage Mediterranean

        // act
        var result = ownership.CanBuildHouse(1, "player-1", PropertyColorGroup.Brown);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBuildHouse_OtherPropertyMortgaged_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")   // Mediterranean Avenue
            .SetOwner(3, "player-1")   // Baltic Avenue
            .Mortgage(3);              // Mortgage Baltic

        // act
        var result = ownership.CanBuildHouse(1, "player-1", PropertyColorGroup.Brown);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CountUnmortgagedInColorGroup_AllUnmortgaged_ReturnsFullCount()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player-1")   // Reading Railroad
            .SetOwner(15, "player-1")  // Pennsylvania Railroad
            .SetOwner(25, "player-1"); // B&O Railroad

        // act
        var result = ownership.CountUnmortgagedInColorGroup("player-1", PropertyColorGroup.Railroad);

        // assert
        result.Should().Be(3);
    }

    [Fact]
    public void CountUnmortgagedInColorGroup_SomeMortgaged_ReturnsReducedCount()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player-1")   // Reading Railroad
            .SetOwner(15, "player-1")  // Pennsylvania Railroad
            .SetOwner(25, "player-1")  // B&O Railroad
            .Mortgage(15);             // Mortgage Pennsylvania

        // act
        var result = ownership.CountUnmortgagedInColorGroup("player-1", PropertyColorGroup.Railroad);

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void RentCalculator_MortgagedProperty_ReturnsZero()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        var square = MonopolyBoardConfiguration.GetSquare(1);

        // act
        var rent = RentCalculator.CalculateRent(square, "player-1", ownership);

        // assert
        rent.Should().Be(0);
    }

    [Fact]
    public void RentCalculator_UnmortgagedProperty_ReturnsRent()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        var square = MonopolyBoardConfiguration.GetSquare(1);

        // act
        var rent = RentCalculator.CalculateRent(square, "player-1", ownership);

        // assert
        rent.Should().Be(2); // Base rent for Mediterranean Avenue
    }

    [Fact]
    public void RentCalculator_MortgagedRailroad_UsesUnmortgagedCount()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(5, "player-1")   // Reading Railroad
            .SetOwner(15, "player-1")  // Pennsylvania Railroad
            .SetOwner(25, "player-1")  // B&O Railroad
            .Mortgage(15);             // Mortgage Pennsylvania

        var square = MonopolyBoardConfiguration.GetSquare(5);

        // act
        var rent = RentCalculator.CalculateRent(square, "player-1", ownership);

        // assert
        rent.Should().Be(50); // $50 for 2 unmortgaged railroads
    }

    [Fact]
    public void RentCalculator_MortgagedUtility_UsesUnmortgagedCount()
    {
        // arrange
        var ownership = new PropertyOwnershipState()
            .SetOwner(12, "player-1")  // Electric Company
            .SetOwner(28, "player-1")  // Water Works
            .Mortgage(28);             // Mortgage Water Works

        var square = MonopolyBoardConfiguration.GetSquare(12);

        // act
        var rent = RentCalculator.CalculateRent(square, "player-1", ownership, diceRoll: 10);

        // assert
        rent.Should().Be(40); // 4x dice roll for 1 unmortgaged utility
    }

    [Fact]
    public void Equality_SameMortgageState_AreEqual()
    {
        // arrange
        var ownership1 = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        var ownership2 = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        // act & assert
        ownership1.Equals(ownership2).Should().BeTrue();
        ownership1.GetHashCode().Should().Be(ownership2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentMortgageState_AreNotEqual()
    {
        // arrange
        var ownership1 = new PropertyOwnershipState()
            .SetOwner(1, "player-1")
            .Mortgage(1);

        var ownership2 = new PropertyOwnershipState()
            .SetOwner(1, "player-1");

        // act & assert
        ownership1.Equals(ownership2).Should().BeFalse();
    }
}
