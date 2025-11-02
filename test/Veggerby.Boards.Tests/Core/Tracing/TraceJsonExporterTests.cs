using System;

using Veggerby.Boards.Internal.Tracing;

namespace Veggerby.Boards.Tests.Core.Tracing;

public class TraceJsonExporterTests
{
    [Fact]
    public void Empty_Trace_Should_Serialize_To_Empty_Array()
    {
        // arrange

        // act

        // assert

        var trace = new EvaluationTrace();

        // Act
        var json = trace.ToJson();

        // Assert
        json.Should().Be("[]");
    }

    [Fact]
    public void Single_Entry_Should_Contain_Kind_And_Order()
    {
        // arrange

        // act

        // assert

        var trace = new EvaluationTrace();
        trace.Add(new TraceEntry(1, "PhaseEnter", "MainPhase", string.Empty, string.Empty, string.Empty, string.Empty, null, 123UL, 456UL, 789UL));

        // Act
        var json = trace.ToJson();

        // Assert
        json.Should().Contain("PhaseEnter");
        json.Should().Contain("\"Order\":1");
    }

    [Fact]
    public void Entries_Should_Remain_In_Appended_Order()
    {
        // arrange

        // act

        // assert

        var trace = new EvaluationTrace();
        trace.Add(new TraceEntry(1, "PhaseEnter", "PhaseA", string.Empty, string.Empty, string.Empty, string.Empty, null, 1, 2, 3));
        trace.Add(new TraceEntry(2, "RuleEvaluated", "PhaseA", "SomeRule", string.Empty, "Valid", "OK", 5, 2, 3, 4));
        trace.Add(new TraceEntry(3, "RuleApplied", "PhaseA", "SomeRule", "SomeEvent", string.Empty, string.Empty, 5, 3, 4, 5));

        // Act
        var json = trace.ToJson();

        // Assert
        var phaseEnterIndex = json.IndexOf("PhaseEnter", StringComparison.Ordinal);
        var ruleEvaluatedIndex = json.IndexOf("RuleEvaluated", StringComparison.Ordinal);
        var ruleAppliedIndex = json.IndexOf("RuleApplied", StringComparison.Ordinal);
        phaseEnterIndex.Should().BeLessThan(ruleEvaluatedIndex);
        ruleEvaluatedIndex.Should().BeLessThan(ruleAppliedIndex);
    }
}
