# Simulation Demo

Demonstrates building Chess and Backgammon games and executing a tiny deterministic sequence.

## Features

- Shows enabling `FeatureFlags.EnableSimulation` (future simulator integration point)
- Applies a few opening chess moves (or prints intent if no direct relation path found)
- Performs a deterministic backgammon dice roll and attempts a single move

## Run

From repo root:

```bash
dotnet run --project samples/SimulationDemo/SimulationDemo.csproj
```

Output lists each applied (or intended) move.

## Notes

- Simplified path resolution: the demo uses only direct tile relations; multi-step knight / long pawn advances may log as intent only.
- Backgammon configuration may not include full rule wiring; dice + piece move attempt are opportunistic.
- This sample is illustrative and not meant as authoritative rules validation.
