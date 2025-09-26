namespace Veggerby.Boards.Internal.Debug;

/// <summary>
/// Internal test hook container for forcing parity mismatch scenarios under the debug parity dual-run evaluator.
/// Not for production use; manipulated only in tests.
/// </summary>
internal static class DebugParityTestHooks
{
    /// <summary>
    /// Gets or sets a value indicating whether to force a reported mismatch regardless of actual equality.
    /// </summary>
    public static bool ForceMismatch { get; set; } = false;
}