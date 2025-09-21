namespace Veggerby.Boards.Core.Artifacts.Patterns;

/// <summary>
/// Defines visit operations for each supported pattern type used in movement and path resolution.
/// </summary>
public interface IPatternVisitor
{
    /// <summary>
    /// Visits a <see cref="FixedPattern"/> containing a predetermined sequence of directions.
    /// </summary>
    /// <param name="pattern">The fixed pattern instance.</param>
    void Visit(FixedPattern pattern);

    /// <summary>
    /// Visits an <see cref="AnyPattern"/> representing zero or more alternative sub-patterns.
    /// </summary>
    /// <param name="pattern">The any pattern instance.</param>
    void Visit(AnyPattern pattern);

    /// <summary>
    /// Visits a <see cref="NullPattern"/> representing a no-op / empty pattern.
    /// </summary>
    /// <param name="pattern">The null pattern instance.</param>
    void Visit(NullPattern pattern);

    /// <summary>
    /// Visits a <see cref="MultiDirectionPattern"/> describing movement across several directions with optional distance bounds.
    /// </summary>
    /// <param name="pattern">The multi-direction pattern instance.</param>
    void Visit(MultiDirectionPattern pattern);

    /// <summary>
    /// Visits a <see cref="DirectionPattern"/> representing movement along a single direction with optional distance bounds.
    /// </summary>
    /// <param name="pattern">The direction pattern instance.</param>
    void Visit(DirectionPattern pattern);
}