using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Reports;
using System.Text;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Generates a consolidated benchmark report (markdown) from one or more BenchmarkDotNet summaries.
/// Responsibility: translate raw benchmark data into structured sections (headings, summary bullets, tables, glossary).
/// </summary>
public static class BenchmarkReportGenerator
{
    /// <summary>
    /// Generates the report at the specified file or directory path.
    /// If <paramref name="outputPath"/> is a directory, a default file name "benchmark-results.md" is used inside it.
    /// Deterministic output ordering by summary title then method descriptor.
    /// </summary>
    /// <param name="summaries">Benchmark summaries to include.</param>
    /// <param name="outputPath">Destination markdown file path or directory.</param>
    public static void Generate(IEnumerable<Summary> summaries, string outputPath)
    {
        if (summaries == null)
        {
            throw new ArgumentNullException(nameof(summaries));
        }

        var materialized = summaries.Where(s => s != null).ToList();
        if (materialized.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path must be provided", nameof(outputPath));
        }

        var resolvedPath = EnsureDirectory(outputPath);

        var builder = new MarkdownBuilder();
        builder.AddHeading(MarkdownBuilder.HeadingLevel.Level1, "Benchmark Results");
        builder.AddParagraph($"_Last generated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} UTC_");
        builder.AddParagraph("This file aggregates performance benchmark summaries produced by BenchmarkDotNet. See performance.md for methodology.");

        builder.AddHeading(MarkdownBuilder.HeadingLevel.Level2, "Distilled Summary");

        var distilledLines = new List<string>();
        foreach (var summary in materialized.OrderBy(s => s.Title))
        {
            foreach (var report in summary.Reports.OrderBy(r => r.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo))
            {
                var descriptor = report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
                var meanNs = report.ResultStatistics?.Mean;
                if (meanNs.HasValue)
                {
                    distilledLines.Add($"{descriptor}: Mean {meanNs.Value / 1000.0:0.###} µs");
                }
                else
                {
                    distilledLines.Add($"{descriptor}: No data (failed or not executed)");
                }
            }
        }
        builder.AddBullets(distilledLines);

        builder.AddHeading(MarkdownBuilder.HeadingLevel.Level2, "Glossary");
        builder.AddBullets(new[]
        {
            "Mean: Arithmetic average time per operation (microseconds). Lower is better.",
            "Error: Half of 99.9% confidence interval (BenchmarkDotNet default).",
            "StdDev: Standard deviation of sampled measurements.",
            "Ratio: Mean(Current)/Mean(Baseline) when a baseline benchmark is defined.",
            "Gen0/Gen1: GC collections per 1000 operations (lower generally better).",
            "Allocated: Managed memory allocated per operation.",
            "Alloc Ratio: Allocation(Current)/Allocation(Baseline)."
        });

        builder.AddHeading(MarkdownBuilder.HeadingLevel.Level2, "Raw Tables");
        foreach (var summary in materialized)
        {
            builder.AddHeading(MarkdownBuilder.HeadingLevel.Level3, summary.Title);
            var headers = summary.Table.Columns.Select(c => c.Header).ToList();
            var rows = summary.Table.FullContent.Select(r => r.ToList()).ToList();
            builder.AddTable(headers, rows);
        }

        builder.AddHeading(MarkdownBuilder.HeadingLevel.Level2, "Artifacts");
        builder.AddParagraph("Detailed per benchmark artifacts (CSV, HTML, md) are under BenchmarkDotNet.Artifacts/results.");

        using (var stream = File.Create(resolvedPath))
        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
        {
            builder.Build(writer);
        }
    }

    private static string EnsureDirectory(string path)
    {
        path = Path.GetFullPath(path);

        var folder = Path.GetDirectoryName(path) ?? "./";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return path;
    }
}

