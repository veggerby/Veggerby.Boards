using System.Linq;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.States;

public class OwnershipStateTests
{
    [Fact]
    public void Constructor_Should_Create_Empty_State()
    {
        // arrange & act
        var state = new OwnershipState<string>("territory");

        // assert
        state.Count.Should().Be(0);
    }

    [Fact]
    public void SetOwner_Should_Create_New_State_With_Owner()
    {
        // arrange
        var state = new OwnershipState<string>("territory");

        // act
        var newState = state.SetOwner("alaska", "player-red");

        // assert
        state.GetOwner("alaska").Should().BeNull(); // Original unchanged
        newState.GetOwner("alaska").Should().Be("player-red");
    }

    [Fact]
    public void IsOwned_Should_Return_True_For_Owned_Item()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        // act
        var result = state.IsOwned("alaska");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOwned_Should_Return_False_For_Unowned_Item()
    {
        // arrange
        var state = new OwnershipState<string>("territory");

        // act
        var result = state.IsOwned("alaska");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOwnedBy_Should_Return_True_For_Correct_Owner()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        // act
        var result = state.IsOwnedBy("alaska", "player-red");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOwnedBy_Should_Return_False_For_Wrong_Owner()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        // act
        var result = state.IsOwnedBy("alaska", "player-blue");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetOwnedBy_Should_Return_All_Items_For_Player()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red")
            .SetOwner("canada", "player-red")
            .SetOwner("brazil", "player-blue");

        // act
        var result = state.GetOwnedBy("player-red").ToList();

        // assert
        result.Should().HaveCount(2);
        result.Should().Contain("alaska");
        result.Should().Contain("canada");
    }

    [Fact]
    public void CountOwnedBy_Should_Return_Correct_Count()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red")
            .SetOwner("canada", "player-red")
            .SetOwner("brazil", "player-blue");

        // act
        var result = state.CountOwnedBy("player-red");

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void TransferOwnership_Should_Change_Owner()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        // act
        var newState = state.TransferOwnership("alaska", "player-blue");

        // assert
        state.GetOwner("alaska").Should().Be("player-red"); // Original unchanged
        newState.GetOwner("alaska").Should().Be("player-blue");
    }

    [Fact]
    public void ClearOwner_Should_Remove_Ownership()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        // act
        var newState = state.ClearOwner("alaska");

        // assert
        newState.GetOwner("alaska").Should().BeNull();
    }

    [Fact]
    public void Equals_Should_Return_True_For_Same_State()
    {
        // arrange
        var state1 = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red")
            .SetOwner("brazil", "player-blue");

        var state2 = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red")
            .SetOwner("brazil", "player-blue");

        // act
        var result = state1.Equals(state2);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Context()
    {
        // arrange
        var state1 = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red");

        var state2 = new OwnershipState<string>("property")
            .SetOwner("alaska", "player-red");

        // act
        var result = state1.Equals(state2);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Works_With_Integer_Keys()
    {
        // arrange
        var state = new OwnershipState<int>("positions");

        // act
        var newState = state
            .SetOwner(1, "player-red")
            .SetOwner(5, "player-blue")
            .SetOwner(10, "player-red");

        // assert
        newState.GetOwner(1).Should().Be("player-red");
        newState.GetOwner(5).Should().Be("player-blue");
        newState.CountOwnedBy("player-red").Should().Be(2);
    }

    [Fact]
    public void GetAll_Should_Return_All_Ownership_Records()
    {
        // arrange
        var state = new OwnershipState<string>("territory")
            .SetOwner("alaska", "player-red")
            .SetOwner("brazil", "player-blue");

        // act
        var result = state.GetAll().ToList();

        // assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(o => o.Key == "alaska" && o.OwnerId == "player-red");
        result.Should().ContainSingle(o => o.Key == "brazil" && o.OwnerId == "player-blue");
    }
}
