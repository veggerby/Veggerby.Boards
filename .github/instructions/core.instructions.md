---
applyTo: "src/Veggerby.Boards/**"
---
Core engine instructions:

1. Do not introduce dependencies beyond the BCL.
2. Preserve immutability: no in-place mutation of `GameState`, artifacts, or internal snapshots.
3. No game-specific (Chess/Backgammon/Go) logic; keep abstractions generic and justified by at least one current or imminent module need.
4. Performance-sensitive paths (bitboards, rule dispatch, pattern resolution) must avoid LINQ and unnecessary allocations; favor explicit loops.
5. All new public types require XML docs (summary + invariants in remarks if non-trivial).
6. Add tests covering: valid path, invalid (exception), and no-op branches for each new rule/mutator.
7. Determinism is mandatory—no ambient randomness; randomness only via `DiceState<T>`.
8. Abstractions must have a concrete in-repo use; do not add speculative layers.
9. Feature flags must be restored after tests using disposable helpers.
10. If modifying bitboard structures, include microbench rationale or maintain parity tests proving no behavioral regression.
11. Game termination must use the standardized `GameEndedState` marker; game modules should implement `IGameOutcome` for outcome tracking (see `docs/game-termination.md`).
