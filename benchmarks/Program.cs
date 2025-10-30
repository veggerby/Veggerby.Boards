using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Running;
using Veggerby.Boards.Benchmarks;

// Benchmark harness entry point.
// Supports:
//   --filter <pattern> (standard BenchmarkDotNet filter)
//   --generate-report / -g (emit distilled markdown summary)
//   --report-out / -o <path> (file or directory for report; directory auto-appends benchmark-results.md)
// Example:
//   dotnet run -c Release -- --filter *BitboardIncrementalBenchmark* --generate-report -o ./benchmarks/docs/custom-output.md

var generateReport = args.GetCommandLineParameter<bool>("--generate-report", false, "-g");
var explicitOut = args.GetCommandLineParameter<string?>("--report-out", null, "-o");

// Strip custom options (we'll normalize filter) before passing to BenchmarkDotNet
var filteredArgs = args
    .Where(a => !string.Equals(a, "--generate-report", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(a, "-g", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(a, "--report-out", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(a, "-o", StringComparison.OrdinalIgnoreCase))
    .ToList();

var summaries = BenchmarkSwitcher.FromAssembly(typeof(SlidingPathResolutionBenchmark).Assembly)
    .Run(filteredArgs.ToArray());

var summariesList = summaries?.ToList();

if (generateReport && summariesList is { Count: > 0 })
{
    var reportPath = !string.IsNullOrWhiteSpace(explicitOut)
        ? explicitOut!
        : Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")), "docs", "benchmark-results.md");

    BenchmarkReportGenerator.Generate(summariesList, reportPath);
    Console.WriteLine($"[benchmark-report] Generated {Path.GetFullPath(reportPath)}");
}
