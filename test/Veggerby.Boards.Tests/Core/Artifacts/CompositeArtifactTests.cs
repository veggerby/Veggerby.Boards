using System;
using System.Linq;


using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Tests.Core.Artifacts;

public class CompositeArtifactTests
{
    public class Constructor
    {
        [Fact]
        public void Should_initialize_properties()
        {
            // arrange
            var tile1 = new Tile("tile1");
            var tile2 = new Tile("tile2");
            var children = new[] { tile1, tile2 };

            // act
            var actual = new CompositeArtifact<Tile>("combined", children);

            // assert
            actual.Id.Should().Be("combined");
            actual.ChildArtifacts.Should().Equal(children);
        }

        [Fact]
        public void Should_throw_when_null_children_are_specified()
        {
            // arrange

            // act
            var actual = () => new CompositeArtifact<Tile>("combined", null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("childArtifacts");
        }

        [Fact]
        public void Should_throw_when_empty_relations_are_specified()
        {
            // arrange

            // act
            var actual = () => new CompositeArtifact<Tile>("combined", Enumerable.Empty<Tile>());

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("childArtifacts");
        }
    }
}