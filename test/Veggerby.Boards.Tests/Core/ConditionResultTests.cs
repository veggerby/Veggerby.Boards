using System;
using Shouldly;
using Veggerby.Boards.Core;
using Xunit;

namespace Veggerby.Boards.Tests.Core
{
    public class ConditionResultTests
    {
        public class Static
        {
            [Fact]
            public void Should_have_static_states()
            {
                // arrange

                // act

                // assert
                ConditionResponse.Valid.ShouldNotBeNull();
                ConditionResponse.Valid.Result.ShouldBe(ConditionResult.Valid);
                ConditionResponse.Valid.Reason.ShouldBeNull();

                ConditionResponse.Invalid.ShouldNotBeNull();
                ConditionResponse.Invalid.Result.ShouldBe(ConditionResult.Invalid);
                ConditionResponse.Invalid.Reason.ShouldBeNull();

                ConditionResponse.NotApplicable.ShouldNotBeNull();
                ConditionResponse.NotApplicable.Result.ShouldBe(ConditionResult.Ignore);
                ConditionResponse.NotApplicable.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_create_valid_state()
            {
                // arrange

                // act
                var actual = ConditionResponse.Success("it worked!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(ConditionResult.Valid);
                actual.Reason.ShouldBe("it worked!");
            }

            [Fact]
            public void Should_create_invalid_state()
            {
                // arrange

                // act
                var actual = ConditionResponse.Fail("it did not work!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(ConditionResult.Invalid);
                actual.Reason.ShouldBe("it did not work!");
            }

            [Fact]
            public void Should_create_ignore_state()
            {
                // arrange

                // act
                var actual = ConditionResponse.Ignore("meh!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(ConditionResult.Ignore);
                actual.Reason.ShouldBe("meh!");
            }

            [Fact]
            public void Should_create_fails_state_composite()
            {
                // arrange
                var state1 = ConditionResponse.Fail("fail");
                var state2 = ConditionResponse.Success("success");
                var state3 = ConditionResponse.Ignore("ignore");

                // act
                var actual = ConditionResponse.Fail(new [] { state1, state2, state3 });

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(ConditionResult.Invalid);
                actual.Reason.ShouldBe("fail,success,ignore");
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
                actual.ShouldBe(expected);
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_return_hashcode()
            {
                // arrange
                var expected = ConditionResult.Valid.GetHashCode();
                var state = ConditionResponse.Success("woo hoo!");

                // act
                var actual = state.GetHashCode();

                // assert
                actual.ShouldBe(expected);
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
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var state = ConditionResponse.Success("success");

                // act
                var actual = state.Equals(null);

                // assert
                actual.ShouldBeFalse();
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
                actual.ShouldBeTrue();
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
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_type()
            {
                // arrange
                var state = ConditionResponse.Success("success");

                // act
                var actual = state.Equals("success");

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}