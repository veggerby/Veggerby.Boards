using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Integration-level parity test ensuring that when compiled patterns are enabled at build time the resolved
/// movement path for a representative chess move (white pawn double advance) matches the legacy visitor result.
/// </summary>
public class ChessCompiledIntegrationParityTests
{
}