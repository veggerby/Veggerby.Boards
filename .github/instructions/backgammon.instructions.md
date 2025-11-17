---
applyTo: "src/Veggerby.Boards.Backgammon/**"
---
Backgammon module instructions:

1. Keep dice handling explicit via `DiceState`; do not embed random generation inside rules.
2. Bearing off, entering from bar, and movement legality must be validated through rules—no direct state mutation helpers.
3. Pip count / race evaluation utilities must be deterministic and allocation-conscious (avoid LINQ in loops evaluating moves).
4. Tests must cover: blocked move, valid move, partial move sequences (when not all dice can be used), bar re-entry edge cases, bearing off boundary conditions.
5. No duplication of generic path / occupancy logic—reuse core bitboard or layout helpers.
6. Keep doubling cube or match score concepts out unless explicitly scoped (feature-flag or documented extension seam).
7. Any performance-sensitive enumeration (move generation) requires benchmark or reasoning if optimized.
8. Stay within module boundary—no leaking Backgammon nomenclature into core.
9. Public surface minimal—prefer internal for helper evaluators unless reused in >=2 rule tests.
10. Update `/docs/backgammon/` if introducing new extension seams or variant mechanics.
11. Use phase-first architecture (see `docs/phase-based-design.md`); consider explicit phases for bar-clearing, dice-rolling, and endgame detection.
12. When adding game termination, use `GameEndedState` and implement `IGameOutcome` for outcome tracking (see `docs/game-termination.md`).
