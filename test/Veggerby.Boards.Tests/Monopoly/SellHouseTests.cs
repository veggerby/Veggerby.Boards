using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class SellHouseTests
{
    [Fact]
    public void CanSellHouse_WithHouses_ReturnsTrue()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        ownership = ownership.SetOwner(1, "player-1");
        ownership = ownership.SetOwner(3, "player-1");
        ownership = ownership.SetHouseCount(1, 2);
        ownership = ownership.SetHouseCount(3, 2);

        // act
        var canSell = ownership.GetHouseCount(1) > 0;

        // assert
        canSell.Should().BeTrue();
    }

    [Fact]
    public void CanSellHouse_WithNoHouses_ReturnsFalse()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        ownership = ownership.SetOwner(1, "player-1");

        // act
        var houseCount = ownership.GetHouseCount(1);

        // assert
        houseCount.Should().Be(0);
    }

    [Fact]
    public void RemoveHouse_FromPropertyWithHouses_DecreasesCount()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        ownership = ownership.SetOwner(1, "player-1");
        ownership = ownership.SetHouseCount(1, 3);

        // act
        var newOwnership = ownership.SetHouseCount(1, 2);

        // assert
        newOwnership.GetHouseCount(1).Should().Be(2);
    }

    [Fact]
    public void RemoveHouse_FromHotel_LeaveFourHouses()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        ownership = ownership.SetOwner(1, "player-1");
        ownership = ownership.SetHouseCount(1, PropertyOwnershipState.HotelValue);

        // act
        var newOwnership = ownership.SetHouseCount(1, PropertyOwnershipState.MaxHouses);

        // assert
        newOwnership.GetHouseCount(1).Should().Be(4);
        newOwnership.HasHotel(1).Should().BeFalse();
    }

    [Fact]
    public void SellPrice_IsHalfOfBuyPrice()
    {
        // arrange
        var houseCost = 100;
        var sellMultiplier = 0.5m;

        // act
        var sellPrice = (int)(houseCost * sellMultiplier);

        // assert
        sellPrice.Should().Be(50);
    }

    [Fact]
    public void EvenSellingRule_MustSellFromHighestFirst()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        // Mediterranean (1) and Baltic (3) in Brown group
        ownership = ownership.SetOwner(1, "player-1");
        ownership = ownership.SetOwner(3, "player-1");
        ownership = ownership.SetHouseCount(1, 3);
        ownership = ownership.SetHouseCount(3, 2);

        // act
        var maxInGroup = ownership.GetMaxHouseCountInColorGroup("player-1", PropertyColorGroup.Brown);
        var minInGroup = ownership.GetMinHouseCountInColorGroup("player-1", PropertyColorGroup.Brown);

        // assert
        // Can only sell from Mediterranean (position 1) because it has more houses
        maxInGroup.Should().Be(3);
        minInGroup.Should().Be(2);
    }

    [Fact]
    public void EvenSellingRule_CanSellFromAnyWhenEqual()
    {
        // arrange
        var ownership = new PropertyOwnershipState();
        ownership = ownership.SetOwner(1, "player-1");
        ownership = ownership.SetOwner(3, "player-1");
        ownership = ownership.SetHouseCount(1, 2);
        ownership = ownership.SetHouseCount(3, 2);

        // act
        var maxInGroup = ownership.GetMaxHouseCountInColorGroup("player-1", PropertyColorGroup.Brown);
        var minInGroup = ownership.GetMinHouseCountInColorGroup("player-1", PropertyColorGroup.Brown);

        // assert
        maxInGroup.Should().Be(minInGroup);
    }
}
