using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class ArtifactTests
    {
        public class _ctor
        {
            [Fact]
            public void Should_create_with_id()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = player.Id;

                // assert
                actual.ShouldBe("player");
            }

            [Fact]
            public void Should_throw_with_empty_id()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentException>(() => new Player(null));

                // assert
                actual.ParamName.ShouldBe("id");
            }
        }

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = player.Equals(player);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = player.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_type_same_id()
            {
                // arrange
                var player1 = new Player("player");
                var player2 = new Player("player");

                // act
                var actual = player1.Equals(player2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_same_type_different_id()
            {
                // arrange
                var player1 = new Player("player1");
                var player2 = new Player("player2");

                // act
                var actual = player1.Equals(player2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_type_same_id()
            {
                // arrange
                var player = new Player("id");
                var dice = new RegularDice("id");

                // act
                var actual = player.Equals(dice);

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}