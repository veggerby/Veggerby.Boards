using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core;

public class GameEngineTests
{
    public class New
    {
        [Fact]
        public void Should_initialize_gameengine()
        {
            // arrange
            var builder = new TestGameBuilder().Compile();
            var game = builder.Game;
            var gamePhaseRoot = GamePhase.New(1, "test", new NullGameStateCondition(), GameEventRule<IGameEvent>.Null);

            // act
            var actual = new GameEngine(game, gamePhaseRoot);

            // assert
            actual.Should().NotBeNull();
            actual.Game.Should().Be(game);
            actual.GamePhaseRoot.Should().Be(gamePhaseRoot);
        }

        [Fact]
        public void Should_throw_with_null_state()
        {
            // arrange
            var gamePhaseRoot = GamePhase.New(1, "test", new NullGameStateCondition(), GameEventRule<IGameEvent>.Null);

            // act
            var actual = () => new GameEngine(null, gamePhaseRoot);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("game");
        }

        [Fact]
        public void Should_throw_with_null_condition()
        {
            // arrange
            var builder = new TestGameBuilder().Compile();
            var game = builder.Game;

            // act
            var actual = () => new GameEngine(game, null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("gamePhaseRoot");
        }
    }
}