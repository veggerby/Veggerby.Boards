using Veggerby.Boards;

namespace Veggerby.Boards.Tests.Core;

public class ConditionResultTests
{
    public class Static
    {
        [Fact]
        public void Should_have_static_states()
        {
            // arrange

            // act (no action – static members)

            // assert
            ConditionResponse.Valid.Should().NotBeNull();
            ConditionResponse.Valid.Result.Should().Be(ConditionResult.Valid);
            ConditionResponse.Valid.Reason.Should().BeNull();

            ConditionResponse.Invalid.Should().NotBeNull();
            ConditionResponse.Invalid.Result.Should().Be(ConditionResult.Invalid);
            ConditionResponse.Invalid.Reason.Should().BeNull();

            ConditionResponse.NotApplicable.Should().NotBeNull();
            ConditionResponse.NotApplicable.Result.Should().Be(ConditionResult.Ignore);
            ConditionResponse.NotApplicable.Reason.Should().BeNull();
        }

        [Fact]
        public void Should_create_valid_state()
        {
            // arrange

            // act
            var actual = ConditionResponse.Success("it worked!");

            // assert
            actual.Should().NotBeNull();
            actual.Result.Should().Be(ConditionResult.Valid);
            actual.Reason.Should().Be("it worked!");
        }

        [Fact]
        public void Should_create_invalid_state()
        {
            // arrange

            // act
            var actual = ConditionResponse.Fail("it did not work!");

            // assert
            actual.Should().NotBeNull();
            actual.Result.Should().Be(ConditionResult.Invalid);
            actual.Reason.Should().Be("it did not work!");
        }

        [Fact]
        public void Should_create_ignore_state()
        {
            // arrange

            // act
            var actual = ConditionResponse.Ignore("meh!");

            // assert
            actual.Should().NotBeNull();
            actual.Result.Should().Be(ConditionResult.Ignore);
            actual.Reason.Should().Be("meh!");
        }

        [Fact]
        public void Should_create_fails_state_composite()
        {
            // arrange
            var state1 = ConditionResponse.Fail("fail");
            var state2 = ConditionResponse.Success("success");
            var state3 = ConditionResponse.Ignore("ignore");

            // act
            var actual = ConditionResponse.Fail([state1, state2, state3]);

            // assert
            actual.Should().NotBeNull();
            actual.Result.Should().Be(ConditionResult.Invalid);
            actual.Reason.Should().Be("fail,success,ignore");
        }
    }

    public class _ToString
    {
        [Theory]
        [InlineData(ConditionResult.Valid, "success", "ConditionResponse Valid/success")]
        [InlineData(ConditionResult.Valid, null, "ConditionResponse Valid/")]
        [InlineData(ConditionResult.Valid, "", "ConditionResponse Valid/")]
        [InlineData(ConditionResult.Ignore, "not relevant", "ConditionResponse Ignore/not relevant")]
        [InlineData(ConditionResult.Invalid, "fail", "ConditionResponse Invalid/fail")]
        public void Should_return_valid_string(ConditionResult result, string reason, string expected)
        {
            // arrange
            var checkState = ConditionResponse.New(result, reason);

            // act
            var actual = checkState.ToString();

            // assert
            actual.Should().Be(expected);
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var state = ConditionResponse.Success("success");

            // act
            var actual = state.Equals(state);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var state = ConditionResponse.Success("success");

            // act
            var actual = state.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_result_different_reason()
        {
            // arrange
            var state1 = ConditionResponse.Success("success");
            var state2 = ConditionResponse.Success("more success");

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_different_result()
        {
            // arrange
            var state1 = ConditionResponse.Success("success");
            var state2 = ConditionResponse.Fail("more success");

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_different_type()
        {
            // arrange
            var state = ConditionResponse.Success("success");

            // act
            var actual = state.Equals("success");

            // assert
            actual.Should().BeFalse();
        }
    }
}