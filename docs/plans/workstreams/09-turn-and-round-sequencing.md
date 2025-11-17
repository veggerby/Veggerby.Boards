---
id: 9
slug: turn-and-round-sequencing
name: "Turn & Round Sequencing"
status: done
last_updated: 2025-11-16
owner: core
flags:
  - EnableTurnSequencing (graduated - default ON)
summary: >-
  Complete turn sequencing infrastructure with TurnState segmentation (advance, pass, commit, replay), 
  Go two-pass integration, centralized active player projection, comprehensive documentation, and full
  module integration validation.
acceptance:
  - TurnState model integrated without breaking existing rule evaluation. ✅
  - Overhead benchmark captured baseline costs (< target threshold documented elsewhere). ✅
  - Go two-pass termination uses TurnState.PassStreak. ✅
  - All modules use centralized active player projection helpers. ✅
  - Comprehensive turn-sequencing.md documentation with lifecycle guide. ✅
  - Hash parity tests validate determinism. ✅
completed:
  - TurnState segmentation model
  - Overhead benchmark instrumentation
  - Integration path w/out evaluation interruptions
  - Documentation updated with comprehensive lifecycle guide, module integration examples, and migration patterns
  - Go two-pass terminal integration via TurnState.PassStreak
  - Active player projection verification across all modules
  - Hash parity tests for deterministic replay
open_followups:
  - Multi-round orchestration API (round summary, phase resets) - deferred to future enhancement
  - Custom segment profiles (e.g., Upkeep → Draw → Action → Buy) - planned for future modules
  - Replay loop guard rails (max depth, diagnostics event) - low priority
  - Sequencing visualization (docs diagram) - nice to have
---

# Turn & Round Sequencing

## Status: COMPLETE ✅

All acceptance criteria met. Turn sequencing is now fully graduated, default-enabled, and integrated across Chess, Backgammon, and Go modules.

## Delivered

- **TurnState segmentation model**: Complete implementation with TurnNumber, Segment (Start/Main/End), and PassStreak
- **Core mutators**: TurnAdvanceStateMutator, TurnPassStateMutator, TurnCommitStateMutator, TurnReplayStateMutator
- **Rotation helper**: TurnSequencingHelpers.ApplyTurnAndRotate for deterministic player rotation
- **Overhead benchmark instrumentation**: Baseline performance costs documented
- **Integration path**: Seamless integration without breaking existing rule evaluation
- **Comprehensive documentation**: 
  - Enhanced `docs/turn-sequencing.md` with complete lifecycle guide
  - Module integration examples (Chess, Go, future Ludo/Monopoly patterns)
  - Active player projection best practices
  - Migration guide from legacy patterns
- **Go two-pass terminal integration**: 
  - PassTurnStateMutator uses TurnState.PassStreak for termination
  - PlaceStoneStateMutator resets PassStreak on stone placement
  - Removed ConsecutivePasses from GoStateExtras
  - Comprehensive regression tests
- **Active player projection verification**: All modules use centralized GetActivePlayer/TryGetActivePlayer helpers
- **Determinism validation**: Hash parity tests confirm deterministic turn advancement

## Test Coverage

- **Core Tests**: TurnSequencingTests, TurnSequencingDeterminismTests, TurnSequencingHashParityTests
- **Go Integration**: 9 tests covering two-pass termination, pass streak management, and reset behavior
- **Module Validation**: All 796 tests passing with turn sequencing enabled by default

## Pending / Deferred

See open_followups in front matter. All core functionality complete; remaining items are future enhancements:

- **Multi-round orchestration API**: Round summary, phase resets (planned for future Monopoly/Risk modules)
- **Custom segment profiles**: Module-specific segment flows (e.g., Upkeep → Draw → Action → Buy)
- **Replay loop guard rails**: Max depth tracking, diagnostics events (low priority)
- **Sequencing visualization**: Documentation diagrams (nice to have)

## Risks

Mitigated: Unbounded replay loops addressed through documentation and recommended patterns. Complex round resets deferred to future work with clear extension points documented.

## Next Steps

Workstream complete. Future enhancements tracked in:
- Issue #8 (Ludo): Custom segment profiles for roll-based track games
- Issue #10 (Monopoly): Replay mechanics for doubles, property turn structure
- Issue #11 (Risk): Multi-phase turn structure (reinforce, attack, fortify)
