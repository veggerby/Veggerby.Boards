---
applyTo: "src/Veggerby.Boards.Go/**"
---
Go module instructions (planned / developing):

1. Liberty counting and capture resolution must be purely functional—derive new state snapshots without mutating prior groups.
2. Ko rule enforcement must be deterministic; represent prior position hashes explicitly—no implicit global caches.
3. Avoid premature caching of group structures; compute on demand unless profiling justifies a persistent representation.
4. Suicide move legality and ko edge cases require explicit tests (legal vs illegal placements, multi-stone capture creating ko, snapback scenarios if supported).
5. Board size flexibility (e.g., 9x9, 13x13, 19x19) must not require core changes—keep scaling logic inside module abstractions.
6. Use segmented bitboards or group structures efficiently; no LINQ inside tight liberty/capture evaluation loops.
7. Time / byo-yomi / scoring variants are out of scope unless feature-flagged and documented.
8. Document any added rule variants in `/docs/go/` including scoring method assumptions (territory vs area) if introduced.
9. Keep module self-contained—no leaking Go-specific group terminology into core.
10. All new abstractions must have a concrete test-driven scenario before introduction.
