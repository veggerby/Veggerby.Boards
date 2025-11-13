using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Tests.TestHelpers;

/// <summary>
/// Test-only helpers to assert non-null contract on artifact retrieval convenience methods.
/// Keeps production code strict while allowing tests to remain concise when TreatWarningsAsErrors is enabled.
/// </summary>
public static class NullabilityTestExtensions
{
    /// <summary>
    /// Ensures the instance is not null (asserting eagerly) and returns the value for fluent usage.
    /// </summary>
    public static T EnsureNotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null) where T : class
    {
        if (value is null)
        {
            throw new Xunit.Sdk.XunitException($"Expected non-null value for {paramName}");
        }

        return value;
    }
}