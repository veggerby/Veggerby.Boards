using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;

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

        // act

        // assert

        var values = Enum.GetValues(typeof(EventRejectionReason)).Cast<EventRejectionReason>().ToArray();

        // act
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
        values.Length.Should().Be(expected.Length);
        values.Should().Contain(expected);
    }
}
