using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using AwesomeAssertions;
using Xunit;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Ensures every canonical chess piece id has both a role and color classification via metadata extras.
/// Guards against drift when adding/removing piece constants or adjusting builder wiring.
/// </summary>
public class ChessPieceClassificationTests
{
    [Fact]
    public void GivenStandardChessGame_WhenBuilt_AllPieceIdsResolveRoleAndColor()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var state = progress.State;
        var roles = state.GetExtras<ChessPieceRolesExtras>();
        var colors = state.GetExtras<ChessPieceColorsExtras>();
        roles.Should().NotBeNull();
        colors.Should().NotBeNull();

        // Derive canonical set from role map itself (builder validated coverage already)
        var allPieceIds = roles!.Roles.Keys.ToArray();

        // act
        var roleColorResolution = allPieceIds
            .Select(pid => new
            {
                Id = pid,
                HasRole = ChessPieceRoles.TryGetRole(roles, pid, out _),
                HasColor = ChessPieceColors.TryGetColor(colors, pid, out _)
            })
            .ToArray();

        // assert
        foreach (var rc in roleColorResolution)
        {
            rc.HasRole.Should().BeTrue($"Missing role mapping for piece id '{rc.Id}'");
            rc.HasColor.Should().BeTrue($"Missing color mapping for piece id '{rc.Id}'");
        }
    }
}