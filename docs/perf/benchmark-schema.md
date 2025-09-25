# Benchmark JSON Schema (Draft)

Status: Draft (Workstream 7) – not yet enforced in CI. Intended for future regression gate comparing current benchmark runs to stored baselines.

## Purpose

Provide a minimal, stable structure for capturing per-benchmark scenario metrics enabling:

- Threshold-based regression detection (latency / allocations)
- Historical trend visualization
- Flag context auditing (ensuring apples-to-apples comparisons)

## Schema (Conceptual)

```json
{
  "version": 1,
  "generatedAtUtc": "2025-09-25T12:34:56Z",
  "commit": {
    "sha": "abc123def",
    "branch": "feat/architecture-and-dx"
  },
  "environment": {
    "runtime": ".NET 9.0.0",
    "os": "linux-x64",
    "cpu": "(logical processor model id masked)",
    "flags": {
      "EnableCompiledPatterns": true,
      "EnableSlidingFastPath": true,
      "EnableBitboards": false,
      "EnableObserverBatching": false,
      "EnableDecisionPlanEventFiltering": true
    }
  },
  "benchmarks": [
    {
      "name": "PathResolution_EmptyRay_FastPath",
      "category": "PathResolution",
      "scenario": "EmptyRay_FastPath",
      "unit": "ns/op",
      "p50": 312.4,
      "p95": 344.1,
      "mean": 318.7,
      "stdDev": 7.2,
      "allocBytes": 0,
      "opsPerSecond": 3130000.0,
      "iterations": 100000,
      "notes": "Allocation-free fast-path; flags: compiled patterns + sliding fast-path."
    }
  ]
}
```

## Field Definitions

| Field | Description | Notes |
|-------|-------------|-------|
| version | Schema version | Increment on breaking schema changes |
| generatedAtUtc | ISO-8601 timestamp | UTC only |
| commit.sha | Git commit SHA | Short or full allowed (prefer full in final) |
| commit.branch | Branch name | For context; baseline comparisons may ignore |
| environment.runtime | .NET runtime version | Ensures comparability |
| environment.os | OS RID | linux-x64 / win-x64 / osx-arm64 |
| environment.cpu | CPU descriptor | Redacted or normalized if necessary |
| environment.flags | Active feature flags map | Must include only relevant engine flags |
| benchmarks[].name | Fully qualified benchmark name | category + scenario + variant |
| benchmarks[].category | Logical grouping | e.g., PathResolution, HandleEvent |
| benchmarks[].scenario | Scenario label | Suffix describing context |
| benchmarks[].unit | Measurement unit | Typically ns/op |
| benchmarks[].p50 / p95 | Latency percentiles | Derived from raw measurements |
| benchmarks[].mean | Mean latency | From BenchmarkDotNet summary |
| benchmarks[].stdDev | Standard deviation | From BenchmarkDotNet summary |
| benchmarks[].allocBytes | Allocated bytes per operation | 0 target for hot paths |
| benchmarks[].opsPerSecond | Throughput | Optional convenience field |
| benchmarks[].iterations | Total measured iterations | For statistical weight |
| benchmarks[].notes | Free-form | Keep concise; avoid stale numeric commentary |

## Validation Rules (Planned)

1. All benchmark names must be unique within file.
2. p50 ≤ p95.
3. allocBytes >= 0.
4. Flags object must contain only known feature flags (reject unknown keys once analyzer exists).
5. Mean within [p50, p95] unless distribution highly skewed (allow configurable tolerance).

## CI Integration (Future)

Planned flow:

1. Run designated benchmark subset in CI (fast pack) → produce JSON.
2. Compare against `baseline/benchmark-baseline.json` (same schema) with configured tolerances (p50 regression ≤2% default; allocBytes must not increase on hot-path categories: PathResolution, HandleEvent, SlidingFastPath).
3. Fail job with diff summary table if regression exceeds threshold.
4. Allow explicit override label or baseline update PR with justification.

## Open Questions

- Should we include GC collection counts? (Deferred; can enlarge schema later)
- Include memory bandwidth metrics? (Likely out-of-scope initial)
- Multi-runtime aggregation? (Future once .NET LTS cross-target added)

## Style Charter Compliance

Document intentionally minimal; no embedding outdated numeric claims in narrative—only structure. Updates MUST maintain determinism of flag set and avoid subjective phrasing.

---
Draft maintained under Workstream 7 until regression gate implementation lands.
