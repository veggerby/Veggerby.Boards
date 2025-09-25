# Developer Experience & Quality Gates

This document consolidates the style charter, quality gates, and workflow expectations for contributing to Veggerby.Boards.
It is normative: if it conflicts with informal comments elsewhere, this file (together with `.github/copilot-instructions.md`) wins.

---
## 1. Style Charter (Authoritative Summary)

All new code MUST follow these rules:

1. File‑scoped namespaces only (e.g. `namespace Veggerby.Boards;`).
2. Explicit braces for every control flow block (no single-line implicit bodies).
3. 4 spaces indentation – no tabs – no trailing whitespace.
4. Private fields: `_camelCase`; public members: `PascalCase`; constants: `PascalCase`.
5. Immutability by default: do not mutate existing `GameState` or artifact state objects.
6. No LINQ in hot paths (evaluation loops, fast-path resolution, observers inside callback dispatch, simulation inner loops, benchmarks per-iteration core). Post‑processing / aggregation after loops may use LINQ.
7. Determinism: same input state + event + feature flag set ⇒ identical resulting state + hashes. Any non-deterministic helper is forbidden unless part of explicit RNG state.
8. Feature flags gate experimental behavior only; flag scopes must restore previous state deterministically (use `FeatureFlagScope`).
9. Allocation discipline: fast-path successes, observer batching flush loops, and inner simulation loops must not allocate per iteration (struct buffers or stack spans only). Benchmarks enforce via allocation columns.
10. Any intentional deviation MUST include an inline `// STYLE-DEVIATION:` comment plus an explanatory CHANGELOG entry under a Temporary Exceptions subsection.

### Hot Path Definition
A hot path is any code executed O(N events) or O(N path resolutions) inside benchmarks or real-time engine loops. Assume hot until profiled otherwise.

---
## 2. Quality Gates Overview

| Gate | Purpose | Tooling | Threshold / Expectation |
|------|---------|---------|-------------------------|
| Build Warnings | Keep surface clean | `TreatWarningsAsErrors=true` | 0 warnings |
| Unit Tests | Functional correctness | xUnit + AwesomeAssertions | 100% pass |
| Property Tests | Behavioral invariants | FsCheck (planned) | ≥99% pass across 10k cases |
| Parity Tests | Deterministic equivalence (legacy vs new) | Dedicated parity suites | Zero divergences |
| Benchmarks (Manual) | Performance baselining | BenchmarkDotNet | Reviewed pre-merge |
| Benchmarks (Future CI) | Regression detection | CI + JSON baselines | <2% degradation (configurable) |
| Allocation Guards | Detect hidden allocations | Benchmark columns / probes | 0 allocs on marked hot paths |
| Style Charter | Consistency & readability | Manual review + future analyzer | Zero unapproved deviations |
| Feature Flags | Safe rollout | `FeatureFlagScope` tests | Deterministic restoration |

---
## 3. Benchmarking Policy

1. Benchmarks live in `/benchmarks` project – never in test project.
2. Scenario naming: `Category_Scenario_Action` (e.g. `PathResolution_EmptyRay_FastPath`).
3. Always include baseline + variant for new optimization work; never merge only optimized variant numbers without baseline.
4. Include descriptive summary in CHANGELOG only when semantics or gating thresholds are introduced (avoid stale numeric micro-results in docs).
5. Performance gating (future): JSON baseline diff with configurable tolerances (default 2%).

---
## 4. Observability & Batching Guidelines

Observer callbacks (even batched) must preserve original ordering semantics. Batching buffer sizes must remain small, fixed-size struct arrays – no dynamic growth lists on hot path. Flushing events must handle partial buffer fill without allocation.

Tracing layers always wrap AFTER batching (decorator order: base observer → batching → trace capture) to avoid redundant trace entries or double buffering.

---
## 5. Simulation Style Addendum

Simulation inner loops forbid:
- LINQ
- Heap allocations per playout step
- Hidden randomness (must flow through RNG state)

Metrics aggregation may allocate once per batch (array resize or histogram) but not inside the critical event application loop.

---
## 6. Feature Flag Discipline

- Flags reside centrally (`FeatureFlags`).
- New flags require: name, XML doc (purpose + default), doc update (`configuration.md`), CHANGELOG bullet.
- Temporary experiment flags slated for removal must get a backlog cleanup item.

Nested flag scopes must be wrapped in `using var scope = new FeatureFlagScope(...);` constructs; manual save/restore is prohibited.

---
## 7. Analyzer / Tooling Roadmap (Planned)

Planned Roslyn analyzers (tracked in backlog Section 7):
- Forbidden `System.Random` usage (suggest RNG abstraction).
- Hot-path LINQ detection (heuristic: method name patterns + `Benchmark` context + `[HotPath]` attribute candidate).
- Direct `FeatureFlags` assignment outside `FeatureFlagScope`.
- Mutable state mutation detection inside engine core namespaces.

These will ship as a separate analyzer project once baseline invariants & performance gates are stable.

---
## 8. Contribution Workflow

1. Implement feature behind flag (if experimental).
2. Add/extend tests (parity + unit + property if applicable).
3. Add benchmark (baseline + variant) for performance-sensitive work.
4. Update docs: config, architecture, relevant concept page.
5. Update CHANGELOG (Added/Changed + Style deviation if any).
6. Run full test suite + targeted benchmarks locally.
7. Submit PR referencing relevant backlog items; ensure plan/backlog docs updated if strategic milestone changed.

---
## 9. Determinism Checklist (Pre-PR)

- No DateTime.UtcNow or environment time usage.
- No `Guid.NewGuid()` in core logic.
- RNG interactions only through captured state within `GameState`.
- No shared mutable static collections.
- Hash comparisons use deterministic, documented algorithm versions.

---
## 10. Temporary Exceptions

(None). Add entries here + CHANGELOG Temporary Exceptions section if a deviation is *intentionally* introduced (include removal plan & target date).

---
## 11. Future Enhancements

- CI benchmark regression gate automation.
- Analyzer package publication.
- Integrated perf dashboard (generated from benchmark JSON) – low priority.
- Automated doc lint (markdown spacing + heading level consistency).

---
Maintainers are empowered to block merges that violate any clause above. When in doubt, lean toward explicitness, determinism, and immutability.

---

## 12. Property Test Acceptance Criteria (Workstream 7 Addition)

All property/invariant style tests (including manual loop variants) MUST:

1. Be deterministic: fixed seed or fixed deterministic sequence (no ambient `Random` without a constant seed).
2. Follow AAA structure (arrange / act / assert) with clear region separation in comments where multi-step setups occur.
3. Use explicit feature flag scoping (`using var scope = new FeatureFlagScope(...)`) – no ambient global flag mutations.
4. Avoid LINQ inside per-iteration hot loops; LINQ allowed only for final aggregation or assertion shaping.
5. Assert exactly one semantic outcome per test (multi-assert allowed if logically atomic and related to the single behavior boundary).
6. For negative invariants, assert both lack of state mutation and stable ownership/value semantics (do not assert only one side).
7. Use canonical helpers (e.g., path resolution helpers) instead of duplicating resolution logic directly.
8. Avoid sleeps or time-based assumptions.
9. Document any intentional tolerance (e.g., randomized attempt loops) with inline rationale.
10. No catch-all exception swallowing; explicitly assert expected exceptions when needed.

---

## 13. Feature Flag Isolation Pattern

Tests manipulating feature flags must:

1. Wrap all modifications in a `FeatureFlagScope` specifying only the flags under test.
2. Nest scopes deterministically (LIFO) – never interleave unrelated mutations.
3. Never set flags directly via static assignment in test bodies; only through the scope API.
4. Restore original state automatically by scope disposal (no manual cleanup).
5. Avoid assertions that depend on unspecified default flag states; explicitly enable/disable required flags.
6. Provide negative test coverage if semantics differ with flag disabled (parity expectation or explicit rejection).
7. Inline comments for each flag explaining purpose in that test scenario.
8. Use separate tests per flag interaction pattern (avoid combinatorial explosion in a single test).
9. Mark exploratory or temporary flag usage with a backlog reference if slated for removal.
10. For performance-sensitive flag combinations, ensure a matching benchmark scenario exists (or backlog item referencing the missing benchmark).

Anti-Patterns (Must Avoid):

- Direct `FeatureFlags.SomeFlag = true;` in test code.
- Reusing an existing scope variable after disposal.
- Assuming default flag state across tests without explicit setup.
- Combining orthogonal flag experiments in one test (reduced diagnosability).

Any deviation requires `// STYLE-DEVIATION:` + backlog reference + CHANGELOG entry under Temporary Exceptions.
