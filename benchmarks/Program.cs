using System;

using BenchmarkDotNet.Running;

using Veggerby.Boards.Benchmarks;

// Benchmark harness entry point.
// Allows filtering via --filter pattern using BenchmarkDotNet's standard syntax.
// Example: dotnet run -c Release -- --filter *SlidingPathResolutionBenchmark*

_ = BenchmarkSwitcher.FromAssembly(typeof(SlidingPathResolutionBenchmark).Assembly)
    .Run(args);