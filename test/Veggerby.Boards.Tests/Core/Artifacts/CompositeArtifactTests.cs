using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
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
                actual.Id.ShouldBe("combined");
                actual.ChildArtifacts.ShouldBe(children);
            }

            [Fact]
            public void Should_throw_when_null_children_are_specified()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new CompositeArtifact<Tile>("combined", null));

                // assert
                actual.ParamName.ShouldBe("childArtifacts");
            }

            [Fact]
            public void Should_throw_when_empty_relations_are_specified()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentException>(() => new CompositeArtifact<Tile>("combined", Enumerable.Empty<Tile>()));

                // assert
                actual.ParamName.ShouldBe("childArtifacts");
            }
        }
    }
}