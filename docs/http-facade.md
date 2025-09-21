# Optional HTTP Facade (Planned)

The original in-repo ASP.NET Core project was removed to keep the core engine lean and focused on deterministic evaluation and composability.

## Rationale for Separate Package

- Keeps engine free of web framework dependencies.
- Allows independent versioning and potential multiple façade flavors (HTTP, gRPC, WebSocket).
- Encourages DTO boundary discipline and explicit versioning (e.g., `Veggerby.Boards.Http.V1`).

## Proposed Package: `Veggerby.Boards.Http`

| Aspect | Approach |
|--------|---------|
| DTO Model | Immutable records; explicit Version namespace (V1) |
| Mapping | Thin mappers from engine artifacts & states to DTOs (no logic) |
| Serialization | System.Text.Json with deterministic ordering (for hashing parity) |
| Dependency | Refer to `Veggerby.Boards` + modules (Chess / Backgammon) only |
| Feature Flags | None – facade remains passive consumer |
| Observer Integration | Optional: capture evaluation trace and expose `/games/{id}/trace` |

## Endpoints (Illustrative)

```txt
GET    /v1/games/{id}
POST   /v1/games (body: game template / module id)
POST   /v1/games/{id}/events (body: event contract)
GET    /v1/games/{id}/state/hash  (after hashing enabled)
GET    /v1/games/{id}/trace       (if trace capture enabled)
```

## Versioning Strategy

- Introduce V1 once hashing + observer trace stable.
- Breaking contract changes -> V2 namespace; keep V1 for at least one minor engine release.
- Provide upgrade notes (mapping changes) in facade README.

## Open Questions

- Event schema evolution policy (additive vs replacement)?
- Do we surface raw rule decisions or only final state transitions?
- Should simulation endpoints exist here or in a separate `Simulation` facade?

## Next Steps (Deferred)

1. Finalize state hashing (engine feature flag complete & stable).
2. Add minimal trace serializer (observer integration).
3. Draft DTOs and open design review.
4. Implement facade in separate repository or subfolder with isolated CI.

-- End of Draft --
