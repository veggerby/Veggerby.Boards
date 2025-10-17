# Core Nullability Remediation Plan

## Objective

Establish a consistent, intention-revealing nullability strategy in core so that absence is always modeled explicitly (Option/Try-pattern/empty collections) and `null` is reserved strictly for documented sentinel cases (e.g., unsuccessful resolution attempts) without leaking surprise nulls to callers.

## Guiding Principles (Adapted Playbook)

1. Parameters are non-null by default; accept `T?` only where semantic absence is fundamental (e.g., optional phase label).
2. Never return `null` for collections or strings; use empty collections / `string.Empty`.
3. Use `null` return only where a lookup / resolution naturally expresses "not found" and is cheap to branch on (e.g., `GetTileRelation`, path resolution). Prefer evolving to `Try*` or `Option<T>` when chaining occurs or ambiguity appears.
4. Minimize `null!` and `default!`; when required (builder definitions before With*), include a clarifying comment.
5. Determinism > convenience; no silent fallbacks that mask invalid states.

## CI / Tooling Enforcement

Hard fail regressions early:

```xml
<!-- Directory.Build.props (ensure present at solution root) -->
<Project>
    <PropertyGroup>
        <Nullable>enable</Nullable>
        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
        <WarningsAsErrors>CS8600;CS8602;CS8603;CS8618;CS8625</WarningsAsErrors>
    </PropertyGroup>
</Project>
```

```editorconfig
# Nullability + guard hygiene
dotnet_diagnostic.CA1062.severity = warning       # Validate public params (consider elevating to error later)
dotnet_diagnostic.MA0004.severity = warning       # (Meziantou) Do not return null enumerables
dotnet_diagnostic.MA0032.severity = warning       # (Meziantou) Use ArgumentNullException.ThrowIfNull
```

Future: elevate selected warnings to errors when noise is low.

## Current Null Usage Inventory (Core)

| Category | Examples | Current Shape | Risk | Action |
|----------|----------|---------------|------|--------|
| Resolution returns (nullable) | `ResolvePathCompiledFirst`, `SimplePatternPathResolver.Resolve`, `SlidingFastPathResolver.Resolve`, `CompiledPathResolverAdapter.Resolve`, `DecisionPlan.ResolvePhase`, `CompositeGamePhase.GetActiveGamePhase`, `GamePhase.GetActiveGamePhase` | Return `TilePath?` / `GamePhase?` | Low (callers branch) | Keep; add `TryResolve*` overloads. Eventually consider `[Obsolete]` on nullable-return convenience if misuse persists. |
| Phase label / exclusivity | `GamePhase.Label`, `GamePhase.ExclusivityGroup` | `string?` | Medium (label often surfaced; empty is fine) | Normalize to non-null (`string`) with empty default; remove nullable unless explicit semantic difference needed. |
| Builder deferred IDs | `*Definition.*Id = null!` | null-forgiving with comment (some missing comments) | Medium (opaque to newcomers) | Add explicit comments referencing initialization contract; consider required members instead. |
| Collections never null | `Board.Tiles`, `Game.Players`, `Piece.Patterns`, etc. | Already non-null enumerables | Low | No change. |
| Path resolution occupancy semantics | Fast-path occupancy filter returns `null` to indicate invalid move | Semantically meaningful | Low | Keep; maybe rename method to `TryApplyOccupancySemantics` with bool out path for clarity. |
| Turn progression | `TurnProfile.Next` returns `TurnSegment?` | Nullable signals sequence end | Low | Keep. Document pattern in XML summary. |
| Decision plan grouping | `string? inferred` then `string.Empty` fallback list | Normalized before storage | Low | Optionally make local variable non-null; no external exposure. |
| Tracing models | `TraceModels` fields (`string? Phase`, etc.) | DTO layer inside internal tracing | Medium (consumers may branch repeatedly) | Normalize trace to empty string; remove nullable unless absence must be distinguished from empty. |
| ConditionResponse.Reason | `string?` | Null vs non-null reason | Low | Keep (absence of reason distinct from empty). |
| EventResult.Message | `string?` | Optional diagnostic | Low | Keep. |

## High-Value Remediations

1. GamePhase: Convert `Label` and `ExclusivityGroup` to non-nullable `string` with `string.Empty` defaults; adjust factories and builder definitions accordingly.
2. Introduce `TryResolve` pattern for path resolution:
    - `bool TryResolvePath(this Game game, Piece piece, Tile from, Tile to, out TilePath path)`.
    - Same for `GameProgress` variant.
    - Keep existing nullable-return method short-term; add `<remarks>` steering high-frequency callers to `TryResolvePath`.
3. Builder definition records: Replace `private set; = null!;` with `required` init properties or enforce a build lifecycle comment (`// set via WithId before use`). Ensure all carry justification comments.
4. Tracing: Change nullable string properties to non-null with empty normalization at capture time.
5. XML documentation: Explicitly note returned nullable semantics (e.g., `TurnProfile.Next`, path resolvers, phase activation) and guidance for callers (branch once, no deep chains).
6. Add internal guard helper `Ensure.NotNull(object, name)` for consistency or rely solely on `ArgumentNullException.ThrowIfNull` (optional; avoid abstraction unless benefit proven).

## Proposed Refactors (Detail)

### 1. GamePhase Non-null Labels

- Change `public string? Label` -> `public string Label { get; }`
- In constructor: `Label = label ?? string.Empty;`
- Same for `ExclusivityGroup` -> `public string ExclusivityGroup { get; } = string.Empty;` and constructor sets if non-null.
- Update decision plan compilation where exclusivity groups are aggregated (already using `string.Empty` fallback).
- Tests expecting null labels adapt to expecting empty string.

### 2. Path Resolution Try-Pattern

Add in `GameExtensions.Paths.cs` (new file):

```csharp
public static bool TryResolvePath(this Game game, Piece piece, Tile from, Tile to, out TilePath path)
{
    // Guard semantics: existing ResolvePathCompiledFirst returns null for invalid inputs; optionally throw here if desired.
    path = game.ResolvePathCompiledFirst(piece, from, to);
    return path is not null;
}

public static bool TryResolvePath(this GameProgress progress, Piece piece, Tile from, Tile to, out TilePath path)
{
    path = progress.ResolvePathCompiledFirst(piece, from, to);
    return path is not null;
}
```

(Guard strategy: maintain permissive null -> false for now; consider stricter argument validation if misuse detected.)

Annotated variant (recommended for compiler flow assistance):

```csharp
using System.Diagnostics.CodeAnalysis;

public static bool TryResolvePath(this Game game, Piece piece, Tile from, Tile to, [NotNullWhen(true)] out TilePath? path)
{
    path = game.ResolvePathCompiledFirst(piece, from, to);
    return path is not null;
}

public static bool TryResolvePath(this GameProgress progress, Piece piece, Tile from, Tile to, [NotNullWhen(true)] out TilePath? path)
{
    path = progress.ResolvePathCompiledFirst(piece, from, to);
    return path is not null;
}
```

### 3. Builder Definition Required Members

Example:

```csharp
public class TileDefinition
{
    public required string TileId { get; init; }
}
```

If mutation via fluent `WithId` is essential, keep current style but add standardized lifecycle comment:

```csharp
public string TileId { get; private set; } = null!; // LIFECYCLE: set by GameBuilder.WithTile() before Build()
```

Standard tag: `LIFECYCLE:` (grep-able) — audit periodically to ensure property is set on all build paths.

Reviewer rule: Any use of `!` in core MUST either be:

- A lifetime initialization point with `// LIFECYCLE:` comment, OR
- Inside generated / deserialization shim (document with `// DESER:`).
Else: reject PR.

Audit all `*Definition` types and apply uniformly.

### 4. Tracing Normalization

Change trace record:

```csharp
internal readonly record struct TraceEvent(
    string Phase,
    string Rule,
    string EventType,
    string ConditionResult,
    string ConditionReason,
    ...)
```

Capture site converts `null` => `string.Empty`.

Add helper normalization (see Edge Normalization below) so trace capture sites call `Normalize.Text(phaseLabel)` etc.

### 5. XML Docs Enhancements

Add `<returns>` clarifications:

- Path resolvers: "Returns a non-null `TilePath` on success; otherwise null indicating no legal path (blocked, invalid pattern)."
- `TurnProfile.Next`: "Returns next segment or null when current is terminal." Include absence semantics.

### 6. Option/Result (Future Consideration)

Defer introducing `Option<T>` until a concrete chaining pain point arises (e.g., multi-stage path + rule gating). Document criteria in `docs/extensibility.md`.

Criteria (introduce Option) when:

- Caller chains ≥2 nullable resolution results.
- Need to preserve rejection reason beyond simple absence.
- Profiling shows branching overhead or readability issues.

## Deferred / Out-of-Scope Now

- Replacing all nullable returns with discriminated unions (overhead unjustified today).
- Introducing analyzers beyond built-in (evaluate later after initial cleanup).

## Execution Plan (Phased)

1. Phase Labels/Exclusivity (low risk): Update `GamePhase`, `CompositeGamePhase`, adjust tests.
2. Tracing normalization: Update trace models and capture sites.
3. Builder definitions: Choose between `required` or comment + audit; implement uniformly.
4. Try-Pattern additions: Add methods, update inner usages in performance-sensitive loops (optional pilot in path finder tests).
5. Doc updates: Modify XML comments + add section in `docs/core-concepts.md` referencing nullability rules.
6. Optional: Add `docs/nullability-core-plan.md` to index in `docs/index.md`.
7. Add reflection-based policy test (public API scan) to fail build on accidental nullable collections/strings.

## Acceptance Criteria

- No public collection or string properties nullable (except explicitly modeled absence like `ConditionResponse.Reason`).
- All `null!` usages justified with lifecycle comment.
- New try-pattern methods available without breaking existing API.
- Tests green and updated for non-null label expectations.
- Documentation updated and cross-linked.
- Reflection test passes: no public `string?` or `IEnumerable<>?` except whitelisted (`ConditionResponse.Reason`, `EventResult.Message`).

## Open Questions

- Should path resolution failure differentiate (blocked vs invalid pattern) now? (Defer until metrics prove utility.)
- Introduce a light `NullObject` for `IGameEventPreProcessor` collections instead of empty enumerations? (Probably unnecessary.)

## Next Steps Checklist

- [ ] Update `GamePhase` / `CompositeGamePhase` non-null labels.
- [ ] Normalize trace models.
- [ ] Audit and comment all `null!` builder properties.
- [ ] Add TryResolvePath extensions.
- [ ] Enhance XML docs for nullable-return methods.
- [ ] Index plan in docs.
- [ ] Add reflection test for public nullability policy.
- [ ] Add Normalize helper utilities.
- [ ] Add CI editorconfig rules.

## Edge Normalization Helpers

Utility class to centralize null -> empty mapping at boundaries:

```csharp
public static class Normalize
{
    public static string Text(string? s) => s ?? string.Empty;
    public static IReadOnlyList<T> List<T>(IEnumerable<T>? xs) => xs?.ToArray() ?? Array.Empty<T>();
}
```

Usage: applied immediately after deserialization / external API ingestion; never store nullable collections/strings internally.

## Attributes Strategy

Leverage BCL attributes to encode flow assumptions instead of sprinkling `!`:

- `[NotNull]` on out parameters that are always set when method returns true.
- `[NotNullWhen(true)]` for boolean-return methods guarding later dereferences.
- `[MemberNotNull]` inside initialization methods that guarantee non-null fields.

Document attribute use in XML remarks when non-trivial.

## Public API Reflection Test (Sketch)

Goal: fail if accidental nullable slips in.
Robust implementation (uses `NullabilityInfoContext` since reflection type equality can't distinguish `string` vs `string?`):

```csharp
using System.Reflection;
using FluentAssertions;

[Fact]
public void PublicSurface_Should_Not_Expose_Nullable_Collections_Or_Strings()
{
    var asm = typeof(Game).Assembly;
    var ctx = new NullabilityInfoContext();
    var whitelist = new HashSet<string>
    {
        typeof(ConditionResponse).FullName + ".Reason",
        typeof(EventResult).FullName + ".Message"
    };
    var offenders = new List<string>();

    foreach (var t in asm.GetExportedTypes())
    {
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var id = t.FullName + "." + p.Name;
            if (whitelist.Contains(id)) { continue; }
            var n = ctx.Create(p);
            // Ban nullable string
            if (p.PropertyType == typeof(string) && n.WriteState == NullabilityState.Nullable)
            {
                offenders.Add(id);
                continue;
            }
            // Ban nullable enumerable (IEnumerable / IReadOnlyCollection / IReadOnlyList / arrays handled separately if desired)
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string) && n.WriteState == NullabilityState.Nullable)
            {
                offenders.Add(id);
            }
        }
    }

    offenders.Should().BeEmpty("no public string?/IEnumerable<>? in API:\n" + string.Join("\n", offenders));
}
```

## TL;DR Reinforced

"Null is a deliberate signal for point lookups or resolution attempt failure; data containers and metadata default to non-null, using empties to express 'no items'. CI + reflection tests + lifecycle comments make regressions visible and cheap to fix."

## Additional EditorConfig Recommendations

Augment earlier rules (adjust severities after initial stabilization):

```editorconfig
dotnet_diagnostic.CA1720.severity = suggestion   # Avoid confusing identifier names
dotnet_diagnostic.MA0007.severity = warning      # Do not return null for Task (future async APIs)
dotnet_diagnostic.MA0049.severity = warning      # Property can be made init-only
```

## IReadOnly Collections Preference

All public collection-returning members should expose `IReadOnlyList<T>` / `IReadOnlyDictionary<TKey,TValue>` (or specialized immutable structs) — never raw `List<T>` or mutable dictionaries. Empty = `Array.Empty<T>()` or pre-sized immutable.

## XML Returns Template

Use this snippet for intentional nullable returns (`T?`):

```xml
<returns>
    Non-null on success; <c>null</c> indicates the attempted resolution failed (e.g., blocked or invalid).
    Callers should branch once and avoid chaining.
</returns>
```

## Checklist Additions

- Decorate `TryResolve*` with `[NotNullWhen(true)]`.
- Switch public collection types to read-only interfaces where not already.
- Apply XML `<returns>` template to all intentional nullable methods.
- Enforce `LIFECYCLE:` comment presence for each `!` usage in core (manual review + optional analyzer later).

---
One-liner: "Null is a deliberate signal of absence in point lookups or resolution attempts, never a surprise default for data containers or metadata."
