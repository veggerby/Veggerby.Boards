using Veggerby.Boards.Flows.LegalMoveGeneration;

namespace Veggerby.Boards.Tests.Core.LegalMoveGeneration;

/// <summary>
/// Tests for move validation diagnostics and localization support.
/// </summary>
public class MoveValidationDiagnosticsTests
{
    [Fact]
    public void GivenPathObstructedReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.PathObstructed;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("piece", "white-rook"),
            ("blocker", "white-bishop"),
            ("tile", "e4")
        );

        // assert
        explanation.Should().Contain("white-rook");
        explanation.Should().Contain("white-bishop");
        explanation.Should().Contain("e4");
        explanation.Should().Contain("obstructed");
    }

    [Fact]
    public void GivenPieceNotOwnedReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.PieceNotOwned;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("piece", "black-pawn"),
            ("owner", "black"),
            ("activePlayer", "white")
        );

        // assert
        explanation.Should().Contain("black-pawn");
        explanation.Should().Contain("black");
        explanation.Should().Contain("white");
        explanation.Should().Contain("belongs");
    }

    [Fact]
    public void GivenDestinationOccupiedReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.DestinationOccupied;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("piece", "white-knight"),
            ("to", "f3"),
            ("occupant", "white-pawn")
        );

        // assert
        explanation.Should().Contain("white-knight");
        explanation.Should().Contain("f3");
        explanation.Should().Contain("white-pawn");
        explanation.Should().Contain("occupied");
    }

    [Fact]
    public void GivenInvalidPatternReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.InvalidPattern;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("piece", "white-rook"),
            ("from", "a1"),
            ("to", "h8")
        );

        // assert
        explanation.Should().Contain("white-rook");
        explanation.Should().Contain("a1");
        explanation.Should().Contain("h8");
        explanation.Should().Contain("pattern");
    }

    [Fact]
    public void GivenWrongPhaseReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.WrongPhase;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("phase", "dice roll phase")
        );

        // assert
        explanation.Should().Contain("dice roll phase");
        explanation.Should().Contain("phase");
    }

    [Fact]
    public void GivenInsufficientResourcesReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.InsufficientResources;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("resource", "dice values"),
            ("required", "1"),
            ("available", "0")
        );

        // assert
        explanation.Should().Contain("dice values");
        explanation.Should().Contain("1");
        explanation.Should().Contain("0");
        explanation.Should().Contain("insufficient");
    }

    [Fact]
    public void GivenRuleViolationReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.RuleViolation;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(
            reason,
            ("rule", "king cannot be in check")
        );

        // assert
        explanation.Should().Contain("king cannot be in check");
        explanation.Should().Contain("rule");
    }

    [Fact]
    public void GivenGameEndedReason_WhenGettingExplanation_ThenReturnsFormattedMessage()
    {
        // arrange
        var reason = RejectionReason.GameEnded;

        // act
        var explanation = MoveValidationDiagnostics.GetExplanation(reason);

        // assert
        explanation.Should().Contain("ended");
    }

    [Fact]
    public void GivenCustomTemplates_WhenGettingExplanation_ThenUsesCustomTemplate()
    {
        // arrange
        var originalTemplates = MoveValidationDiagnostics.Templates;

        try
        {
            // Replace with custom template
            MoveValidationDiagnostics.Templates = new()
            {
                [RejectionReason.PathObstructed] = "Custom: {piece} blocked by {blocker}"
            };

            var reason = RejectionReason.PathObstructed;

            // act
            var explanation = MoveValidationDiagnostics.GetExplanation(
                reason,
                ("piece", "rook"),
                ("blocker", "bishop")
            );

            // assert
            explanation.Should().Be("Custom: rook blocked by bishop");
        }
        finally
        {
            // Restore original templates
            MoveValidationDiagnostics.Templates = originalTemplates;
        }
    }
}
