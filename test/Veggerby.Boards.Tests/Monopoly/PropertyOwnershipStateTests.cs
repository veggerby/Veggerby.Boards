using System.Linq;

using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class PropertyOwnershipStateTests
{
    [Fact]
    public void Constructor_EmptyOwnership_NoPropertiesOwned()
    {
        // arrange

        // act
        var state = new PropertyOwnershipState();

        // assert
        state.GetOwner(1).Should().BeNull();
        state.IsOwned(1).Should().BeFalse();
    }

    [Fact]
    public void SetOwner_SetsOwnership()
    {
        // arrange
        var state = new PropertyOwnershipState();

        // act
        var newState = state.SetOwner(1, "player1");

        // assert
        newState.GetOwner(1).Should().Be("player1");
        newState.IsOwned(1).Should().BeTrue();
    }

    [Fact]
    public void SetOwner_IsImmutable()
    {
        // arrange
        var state = new PropertyOwnershipState();

        // act
        var newState = state.SetOwner(1, "player1");

        // assert
        state.GetOwner(1).Should().BeNull();
        newState.GetOwner(1).Should().Be("player1");
    }

    [Fact]
    public void SetOwner_TransfersOwnership()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var newState = state.SetOwner(1, "player2");

        // assert
        newState.GetOwner(1).Should().Be("player2");
    }

    [Fact]
    public void GetPropertiesOwnedBy_ReturnsCorrectProperties()
    {
        // arrange
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1")
            .SetOwner(5, "player2");

        // act
        var properties = state.GetPropertiesOwnedBy("player1").ToList();

        // assert
        properties.Should().HaveCount(2);
        properties.Should().Contain(1);
        properties.Should().Contain(3);
    }

    [Fact]
    public void CountOwnedInColorGroup_ReturnsCorrectCount()
    {
        // arrange
        // Brown properties are at positions 1 and 3
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1");

        // act
        var count = state.CountOwnedInColorGroup("player1", PropertyColorGroup.Brown);

        // assert
        count.Should().Be(2);
    }

    [Fact]
    public void HasMonopoly_WithAllProperties_ReturnsTrue()
    {
        // arrange
        // Brown properties are at positions 1 and 3
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player1");

        // act
        var hasMonopoly = state.HasMonopoly("player1", PropertyColorGroup.Brown);

        // assert
        hasMonopoly.Should().BeTrue();
    }

    [Fact]
    public void HasMonopoly_WithPartialOwnership_ReturnsFalse()
    {
        // arrange
        // Brown properties are at positions 1 and 3
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1");

        // act
        var hasMonopoly = state.HasMonopoly("player1", PropertyColorGroup.Brown);

        // assert
        hasMonopoly.Should().BeFalse();
    }

    [Fact]
    public void HasMonopoly_WithMixedOwnership_ReturnsFalse()
    {
        // arrange
        // Brown properties are at positions 1 and 3
        var state = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player2");

        // act
        var hasMonopoly = state.HasMonopoly("player1", PropertyColorGroup.Brown);

        // assert
        hasMonopoly.Should().BeFalse();
    }

    [Fact]
    public void Equals_SameOwnership_ReturnsTrue()
    {
        // arrange
        var state1 = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player2");
        var state2 = new PropertyOwnershipState()
            .SetOwner(1, "player1")
            .SetOwner(3, "player2");

        // act
        var result = state1.Equals((IArtifactState)state2);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentOwnership_ReturnsFalse()
    {
        // arrange
        var state1 = new PropertyOwnershipState()
            .SetOwner(1, "player1");
        var state2 = new PropertyOwnershipState()
            .SetOwner(1, "player2");

        // act
        var result = state1.Equals((IArtifactState)state2);

        // assert
        result.Should().BeFalse();
    }
}
