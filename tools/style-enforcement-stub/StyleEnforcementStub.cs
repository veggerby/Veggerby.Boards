// Simple style enforcement stub.
// Purpose: Provide a lightweight guard until full Roslyn analyzers are implemented.
// This tool scans source files for disallowed constructs defined by the repository style charter.
// NOT a replacement for analyzers; kept intentionally minimal & deterministic.
// Usage (manual): dotnet run --project tools/style-enforcement-stub (future). For now compiled ad-hoc if included in a csproj.
// Invariants enforced:
//  - No tab characters (\t)
//  - No 'goto ' usage
//  - No direct usage of System.Random (must use IRandomSource) outside benchmarks & tests
//  - File-scoped namespace presence (heuristic: 'namespace ' followed by semicolon in first 40 lines) – warning only
//  - Explicit braces: heuristic ensures 'if (' lines not ending with ';' without following '{' on same line (best-effort)
// False positives acceptable initially; any suppression requires inline comment '// STYLE-DEVIATION:' and is reported.
// NOTE: This stub does not modify files – it only reports.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal static class StyleEnforcementStub
{
    private static readonly string[] AllowedRandomPaths = new[] { "/benchmarks/", "/test/" };

    public static int Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        var csFiles = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        int violations = 0;
        foreach (var file in csFiles)
        {
            var rel = file.Replace(root, string.Empty);
            var text = File.ReadAllText(file);

            static bool suppressed(string line) => line.Contains("STYLE-DEVIATION:");

            if (text.Contains('\t'))
            {
                Console.WriteLine($"TAB: {rel}");
                violations++;
            }

            if (text.Contains("goto "))
            {
                Console.WriteLine($"GOTO: {rel}");
                violations++;
            }

            if (text.Contains("System.Random"))
            {
                bool allowed = AllowedRandomPaths.Any(a => rel.Contains(a));
                if (!allowed && !text.Contains("STYLE-DEVIATION:"))
                {
                    Console.WriteLine($"RANDOM: {rel}");
                    violations++;
                }
            }

            // File-scoped namespace heuristic
            var firstLines = text.Split('\n').Take(40).ToList();
            bool hasFileScoped = firstLines.Any(l => l.TrimStart().StartsWith("namespace ") && l.TrimEnd().EndsWith(";"));
            if (!hasFileScoped)
            {
                Console.WriteLine($"WARN (namespace): {rel} lacks file-scoped namespace (heuristic)");
            }

            // Simple explicit braces heuristic
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("if (") && line.TrimEnd().EndsWith(")") && (i + 1 < lines.Length && !lines[i + 1].TrimStart().StartsWith("{")))
                {
                    if (!suppressed(line))
                    {
                        Console.WriteLine($"IF-BRACES: {rel}:{i + 1}");
                        violations++;
                    }
                }
            }
        }

        Console.WriteLine($"Style enforcement stub complete. Violations: {violations}");
        return violations == 0 ? 0 : 1;
    }
}
