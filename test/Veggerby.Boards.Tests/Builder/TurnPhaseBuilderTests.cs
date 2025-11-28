using Veggerby.Boards.Builder.Phases;

namespace Veggerby.Boards.Tests.Builder;

public class TurnPhaseBuilderTests
{
    [Fact]
    public void Create_Should_Return_New_Builder()
    {
        // arrange & act
        var builder = TurnPhaseBuilder.Create();

        // assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void AddPhase_Should_Add_Phase()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.AddPhase("attack");
        var phases = builder.GetPhases();

        // assert
        phases.Should().ContainSingle();
        phases[0].Name.Should().Be("attack");
    }

    [Fact]
    public void AddPhase_Should_Support_Optional_Flag()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.AddPhase("attack", optional: true);
        var phases = builder.GetPhases();

        // assert
        phases[0].Optional.Should().BeTrue();
    }

    [Fact]
    public void AddPhase_Should_Support_Repeatable_Flag()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.AddPhase("attack", repeatable: true);
        var phases = builder.GetPhases();

        // assert
        phases[0].Repeatable.Should().BeTrue();
    }

    [Fact]
    public void WithPlayerRotation_Should_Enable_Rotation()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.AddPhase("move").WithPlayerRotation();

        // assert
        builder.RotatePlayerAfterTurn.Should().BeTrue();
    }

    [Fact]
    public void WithoutPlayerRotation_Should_Disable_Rotation()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.AddPhase("move").WithoutPlayerRotation();

        // assert
        builder.RotatePlayerAfterTurn.Should().BeFalse();
    }

    [Fact]
    public void WithTurnLabel_Should_Set_Label()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        builder.WithTurnLabel("my-turn").AddPhase("move");

        // assert
        builder.TurnLabel.Should().Be("my-turn");
    }

    [Fact]
    public void Build_Should_Create_Configuration()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create()
            .WithTurnLabel("test-turn")
            .AddPhase("reinforce")
            .AddPhase("attack", optional: true)
            .AddPhase("fortify", optional: true)
            .WithPlayerRotation();

        // act
        var config = builder.Build();

        // assert
        config.TurnLabel.Should().Be("test-turn");
        config.Phases.Should().HaveCount(3);
        config.RotatePlayerAfterTurn.Should().BeTrue();
    }

    [Fact]
    public void Build_Should_Throw_If_No_Phases()
    {
        // arrange
        var builder = TurnPhaseBuilder.Create();

        // act
        var act = () => builder.Build();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetPhaseIndex_Should_Return_Correct_Index()
    {
        // arrange
        var config = TurnPhaseBuilder.Create()
            .AddPhase("reinforce")
            .AddPhase("attack", optional: true)
            .AddPhase("fortify", optional: true)
            .Build();

        // act & assert
        config.GetPhaseIndex("reinforce").Should().Be(0);
        config.GetPhaseIndex("attack").Should().Be(1);
        config.GetPhaseIndex("fortify").Should().Be(2);
        config.GetPhaseIndex("nonexistent").Should().Be(-1);
    }

    [Fact]
    public void GetNextPhase_Should_Return_Next_Phase()
    {
        // arrange
        var config = TurnPhaseBuilder.Create()
            .AddPhase("reinforce")
            .AddPhase("attack", optional: true)
            .AddPhase("fortify", optional: true)
            .Build();

        // act & assert
        config.GetNextPhase("reinforce").Should().Be("attack");
        config.GetNextPhase("attack").Should().Be("fortify");
        config.GetNextPhase("fortify").Should().BeNull();
    }

    [Fact]
    public void IsOptional_Should_Return_Correct_Value()
    {
        // arrange
        var config = TurnPhaseBuilder.Create()
            .AddPhase("reinforce")
            .AddPhase("attack", optional: true)
            .AddPhase("fortify", optional: true)
            .Build();

        // act & assert
        config.IsOptional("reinforce").Should().BeFalse();
        config.IsOptional("attack").Should().BeTrue();
        config.IsOptional("fortify").Should().BeTrue();
    }

    [Fact]
    public void IsRepeatable_Should_Return_Correct_Value()
    {
        // arrange
        var config = TurnPhaseBuilder.Create()
            .AddPhase("attack", repeatable: true)
            .AddPhase("end")
            .Build();

        // act & assert
        config.IsRepeatable("attack").Should().BeTrue();
        config.IsRepeatable("end").Should().BeFalse();
    }

    [Fact]
    public void Fluent_Chaining_Should_Work()
    {
        // arrange & act
        var config = TurnPhaseBuilder.Create()
            .WithTurnLabel("test")
            .AddPhase("phase1")
            .AddPhase("phase2", optional: true)
            .AddPhase("phase3", repeatable: true)
            .WithPlayerRotation()
            .Build();

        // assert
        config.TurnLabel.Should().Be("test");
        config.Phases.Should().HaveCount(3);
        config.RotatePlayerAfterTurn.Should().BeTrue();
    }
}
