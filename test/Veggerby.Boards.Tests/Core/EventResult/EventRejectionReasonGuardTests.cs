using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;

using Xunit;

namespace Veggerby.Boards.Tests.Core.EventResult;

/// <summary>
/// Guard test ensuring every <see cref="EventRejectionReason"/> enum value (except EngineInvariant) is explicitly asserted so that
/// adding a new value requires updating this test. Prevents silent fall-through mappings.
/// </summary>
public class EventRejectionReasonGuardTests
{
    [Fact]
    public void GivenEventRejectionReasonEnum_WhenEnumerated_ThenAllKnownValuesAccountedFor()
    {
        // arrange
        var values = Enum.GetValues(typeof(EventRejectionReason)).Cast<EventRejectionReason>().ToArray();

        // act
        // Explicit list - update alongside enum.
        var expected = new[]
        {
            EventRejectionReason.None,
            EventRejectionReason.PhaseClosed,
            EventRejectionReason.NotApplicable,
            EventRejectionReason.InvalidOwnership,
            EventRejectionReason.PathNotFound,
            EventRejectionReason.RuleRejected,
            EventRejectionReason.InvalidEvent,
            EventRejectionReason.EngineInvariant,
        };

        // assert
        Assert.Equal(expected.Length, values.Length);
        Assert.True(expected.All(values.Contains), "Enum changed: update mapping test and related classifier logic.");
    }
}