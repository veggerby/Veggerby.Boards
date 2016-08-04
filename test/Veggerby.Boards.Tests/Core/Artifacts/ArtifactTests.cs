using Veggerby.Boards.Core.Artifacts;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class ArtifactTests
    {
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
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = player.Equals(null);

                // assert
                Assert.False(actual);
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
                Assert.True(actual);
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
                Assert.False(actual);
            }    
            
            [Fact]
            public void Should_not_equal_different_type_same_id()
            {
                // arrange
                var player = new Player("id");
                var die = new RegularDie("id");

                // act
                var actual = player.Equals(die);

                // assert
                Assert.False(actual);
            }        
        }
    }
}