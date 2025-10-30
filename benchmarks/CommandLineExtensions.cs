using System;
using System.ComponentModel;
using System.Globalization;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Lightweight helper extensions for parsing simple command line parameters without introducing external dependencies.
/// </summary>
/// <remarks>
/// Supported forms:
/// <list type="bullet">
/// <item><description>Boolean flags: <c>--generate-report</c> (returns true if present, otherwise false / provided default).</description></item>
/// <item><description>Key + value pairs: <c>--report-out ./path/file.md</c> (<c>./path/file.md</c> captured as the value).</description></item>
/// <item><description>Aliases: any additional names passed after the primary name are treated as equivalent (e.g. <c>-g</c>).</description></item>
/// </list>
/// Parsing is intentionally minimal: it ignores unmatched names and will not throw on parse failure— instead it returns the provided default.
/// A value token is only consumed if the following argument does not begin with <c>-</c>; this prevents accidentally treating another flag as a value.
/// </remarks>
internal static class CommandLineExtensions
{
    /// <summary>
    /// Retrieves a typed parameter or flag value from the argument list.
    /// </summary>
    /// <typeparam name="T">Target type. <see cref="bool"/> is treated as a flag (presence => true).</typeparam>
    /// <param name="args">Command line arguments.</param>
    /// <param name="primaryName">Primary long name (e.g. <c>--filter</c>).</param>
    /// <param name="defaultValue">Default value when flag / parameter not present or parse fails.</param>
    /// <param name="aliases">Optional additional names (short forms etc.).</param>
    /// <returns>The parsed value or <paramref name="defaultValue"/>.</returns>
    public static T? GetCommandLineParameter<T>(this string[] args, string primaryName, T? defaultValue = default, params string[] aliases)
    {
        if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(primaryName))
        {
            return defaultValue;
        }

        // Aggregate all names we should consider
        var names = aliases == null || aliases.Length == 0
            ? new[] { primaryName }
            : Combine(primaryName, aliases);

        var isBool = typeof(T) == typeof(bool);

        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (!IsMatch(current, names))
            {
                continue;
            }

            if (isBool)
            {
                return (T)(object)true; // presence is enough
            }

            // Need a following token that is not another flag
            if (i + 1 >= args.Length)
            {
                return defaultValue;
            }

            var candidate = args[i + 1];
            if (candidate.StartsWith("-"))
            {
                return defaultValue; // treat next flag as not a value
            }

            if (TryConvert(candidate, out T? converted))
            {
                return converted;
            }

            return defaultValue;
        }

        // Flag not present: bool => default (often false), others => default
        return defaultValue;
    }

    private static bool IsMatch(string arg, string[] names)
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (string.Equals(arg, names[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string[] Combine(string primary, string[] aliases)
    {
        var combined = new string[aliases.Length + 1];
        combined[0] = primary;
        for (var i = 0; i < aliases.Length; i++)
        {
            combined[i + 1] = aliases[i];
        }

        return combined;
    }

    private static bool TryConvert<T>(string raw, out T? value)
    {
        try
        {
            // Direct string cast
            if (typeof(T) == typeof(string))
            {
                value = (T?)(object?)raw;
                return true;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, raw);
                value = (T?)converted;
                return true;
            }
        }
        catch
        {
            // swallow – fallback below
        }

        value = default;
        return false;
    }
}
