---
applyTo: "src/Veggerby.Boards.Chess/**"
---
Chess module instructions:

1. Keep logic declarative: use builder patterns to define pieces, movement patterns, and rules without embedding engine-level shortcuts.
2. Do not leak chess-specific concepts (files/ranks, castling, en passant) into core namespaces.
3. Movement patterns must remain deterministic and reproducible; no heuristic pruning in module code.
4. Any new rule (e.g., variant support) must include tests for: valid move, invalid move, ignore (not applicable), and edge cases (e.g., pinned pieces, check conditions) where relevant.
5. Avoid premature optimization—prefer clarity first; add benchmarks only if a demonstrated hotspot emerges.
6. Enforce invariants: illegal king exposure must be prevented at rule evaluation, not retroactively fixed.
7. Use existing bitboard/layout helpers; do not reimplement occupancy logic.
8. All new algebraic/convenience parsing helpers must stay internal unless justified by multiple tests or cross-module reuse.
9. No external dependencies; chess module shares dependency policy of core.
10. Document any novel movement or rule extension in `/docs/chess/` if it broadens extension seams.
