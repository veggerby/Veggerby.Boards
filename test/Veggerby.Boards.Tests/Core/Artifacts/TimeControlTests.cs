using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Tests.Core.Artifacts;

public class TimeControlTests
{
    public class FischerTimeControl
    {
        [Fact]
        public void Should_create_fischer_time_control()
        {
            // arrange

            // act
            var actual = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5),
                Increment = TimeSpan.FromSeconds(2)
            };

            // assert
            actual.InitialTime.Should().Be(TimeSpan.FromMinutes(5));
            actual.Increment.Should().Be(TimeSpan.FromSeconds(2));
            actual.Delay.Should().BeNull();
            actual.MovesPerTimeControl.Should().BeNull();
            actual.BonusTime.Should().BeNull();
        }
    }

    public class BronsteinTimeControl
    {
        [Fact]
        public void Should_create_bronstein_time_control()
        {
            // arrange

            // act
            var actual = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5),
                Delay = TimeSpan.FromSeconds(3)
            };

            // assert
            actual.InitialTime.Should().Be(TimeSpan.FromMinutes(5));
            actual.Delay.Should().Be(TimeSpan.FromSeconds(3));
            actual.Increment.Should().BeNull();
        }
    }

    public class ClassicalTimeControl
    {
        [Fact]
        public void Should_create_classical_time_control()
        {
            // arrange

            // act
            var actual = new TimeControl
            {
                InitialTime = TimeSpan.FromHours(2),
                MovesPerTimeControl = 40,
                BonusTime = TimeSpan.FromMinutes(30)
            };

            // assert
            actual.InitialTime.Should().Be(TimeSpan.FromHours(2));
            actual.MovesPerTimeControl.Should().Be(40);
            actual.BonusTime.Should().Be(TimeSpan.FromMinutes(30));
        }
    }

    public class Equality
    {
        [Fact]
        public void Should_be_equal_when_same_values()
        {
            // arrange
            var control1 = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5),
                Increment = TimeSpan.FromSeconds(2)
            };

            var control2 = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5),
                Increment = TimeSpan.FromSeconds(2)
            };

            // act
            var actual = control1.Equals(control2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_be_equal_when_different_initial_time()
        {
            // arrange
            var control1 = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var control2 = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(10)
            };

            // act
            var actual = control1.Equals(control2);

            // assert
            actual.Should().BeFalse();
        }
    }
}
