namespace Veggerby.Boards.Artifacts.Patterns;

/// <summary>
/// Abstraction for movement patterns that can be visited and resolved into concrete paths.
/// </summary>
public interface IPattern
{
    /// <summary>
    /// Accepts a visitor for pattern-specific resolution logic.
    /// </summary>
    void Accept(IPatternVisitor visitor);
}