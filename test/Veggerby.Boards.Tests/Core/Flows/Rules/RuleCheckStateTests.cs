using System;
using Shouldly;
using Veggerby.Boards.Core.Flows.Rules;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Rules
{
    public class RuleCheckStateTests
    {
        public class Static
        {
            [Fact]
            public void Should_have_static_states()
            {
                // arrange

                // act

                // assert
                RuleCheckState.Valid.ShouldNotBeNull();
                RuleCheckState.Valid.Result.ShouldBe(RuleCheckResult.Valid);
                RuleCheckState.Valid.Reason.ShouldBeNull();

                RuleCheckState.Invalid.ShouldNotBeNull();
                RuleCheckState.Invalid.Result.ShouldBe(RuleCheckResult.Invalid);
                RuleCheckState.Invalid.Reason.ShouldBeNull();

                RuleCheckState.NotApplicable.ShouldNotBeNull();
                RuleCheckState.NotApplicable.Result.ShouldBe(RuleCheckResult.Ignore);
                RuleCheckState.NotApplicable.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_create_valid_state()
            {
                // arrange

                // act
                var actual = RuleCheckState.Success("it worked!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBe("it worked!");
            }

            [Fact]
            public void Should_create_invalid_state()
            {
                // arrange

                // act
                var actual = RuleCheckState.Fail("it did not work!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("it did not work!");
            }

            [Fact]
            public void Should_create_ignore_state()
            {
                // arrange

                // act
                var actual = RuleCheckState.Ignore("meh!");

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(RuleCheckResult.Ignore);
                actual.Reason.ShouldBe("meh!");
            }

            [Fact]
            public void Should_create_fails_state_composite()
            {
                // arrange
                var state1 = RuleCheckState.Fail("fail");
                var state2 = RuleCheckState.Success("success");
                var state3 = RuleCheckState.Ignore("ignore");

                // act
                var actual = RuleCheckState.Fail(new [] { state1, state2, state3 });

                // assert
                actual.ShouldNotBeNull();
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("fail,success,ignore");
            }
        }

        public class _ToString
        {
            [Theory]
            [InlineData(RuleCheckResult.Valid, "success", "RuleCheckState Valid/success")]
            [InlineData(RuleCheckResult.Valid, null, "RuleCheckState Valid/")]
            [InlineData(RuleCheckResult.Valid, "", "RuleCheckState Valid/")]
            [InlineData(RuleCheckResult.Ignore, "not relevant", "RuleCheckState Ignore/not relevant")]
            [InlineData(RuleCheckResult.Invalid, "fail", "RuleCheckState Invalid/fail")]
            public void Should_return_valid_string(RuleCheckResult result, string reason, string expected)
            {
                // arrange
                var checkState = RuleCheckState.New(result, reason);

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
                var expected = RuleCheckResult.Valid.GetHashCode();
                var state = RuleCheckState.Success("woo hoo!");

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
                var state = RuleCheckState.Success("success");

                // act
                var actual = state.Equals(state);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var state = RuleCheckState.Success("success");

                // act
                var actual = state.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_result_different_reason()
            {
                // arrange
                var state1 = RuleCheckState.Success("success");
                var state2 = RuleCheckState.Success("more success");

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_different_result()
            {
                // arrange
                var state1 = RuleCheckState.Success("success");
                var state2 = RuleCheckState.Fail("more success");

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_type()
            {
                // arrange
                var state = RuleCheckState.Success("success");

                // act
                var actual = state.Equals("success");

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}