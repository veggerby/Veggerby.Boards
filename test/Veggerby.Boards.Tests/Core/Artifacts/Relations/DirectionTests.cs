using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations
{
    public class DirectionTests
    {
        public class Statics
        {
            [Fact]
            public void Should_contain_static_properties()
            {
                // arrange
                // act
                // assert
                Direction.Left.ShouldNotBeNull();
                Direction.Right.ShouldNotBeNull();
                Direction.Up.ShouldNotBeNull();
                Direction.Down.ShouldNotBeNull();
                Direction.Across.ShouldNotBeNull();

                Direction.North.ShouldNotBeNull();
                Direction.South.ShouldNotBeNull();
                Direction.East.ShouldNotBeNull();
                Direction.West.ShouldNotBeNull();

                Direction.NorthWest.ShouldNotBeNull();
                Direction.NorthEast.ShouldNotBeNull();
                Direction.SouthWest.ShouldNotBeNull();
                Direction.SouthEast.ShouldNotBeNull();

                Direction.Clockwise.ShouldNotBeNull();
                Direction.CounterClockwise.ShouldNotBeNull();

                Direction.Any.ShouldBeOfType<AnyDirection>();
            }
        }

        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new Direction("id");

                // assert
                actual.Id.ShouldBe("id");
            }

            [Fact]
            public void Should_throw_from_constructor_no_id()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentException>(() => new Direction(string.Empty));

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
                var direction = new Direction("dir");

                // act
                var actual = direction.Equals(direction);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var direction = new Direction("dir");

                // act
                var actual = direction.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_type_same_id()
            {
                // arrange
                var direction1 = new Direction("dir");
                var direction2 = new Direction("dir");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_same_type_different_id()
            {
                // arrange
                var direction1 = new Direction("dir1");
                var direction2 = new Direction("dir2");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_any_direction()
            {
                // arrange
                var direction = new Direction("dir1");

                // act
                var actual = direction.Equals(Direction.Any);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_equal_any_other_direction()
            {
                // arrange
                var direction1 = new AnyDirection();
                var direction2 = new Direction("dir2");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_other_type()
            {
                // arrange
                var direction = new Direction("dir");

                // act
                var actual = direction.Equals("some string");

                // assert
                actual.ShouldBeFalse();
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_return_hashcode_from_id()
            {
                // arrange
                var id = "south";
                var expected = id.GetHashCode();
                var direction = new Direction(id);

                // act
                var actual = direction.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }
        }

        public class _ToString
        {
            [Fact]
            public void Should_return_expected()
            {
                // arrange
                var direction = new Direction("south");

                // act
                var actual = direction.ToString();

                // assert
                actual.ShouldBe("Direction south");
            }
        }
    }
}