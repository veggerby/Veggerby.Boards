# Candidate Board Games

This document outlines a curated set of board games that **Veggerby.Boards** should aim to support.
The list spans from **simple classics** to **complex modern strategy games**, ensuring broad coverage for testing engine capabilities, demonstrating flexibility, and providing recognizable examples for contributors and users.

---

## 🎲 Simple / Classic

These are minimal-rule games that help validate the **core mechanics** of tiles, turns, and win conditions.

* **Checkers (Draughts)** – simple grid, capture jumps.
* **Tic-Tac-Toe** – trivial grid; useful as a demo and smoke test.
* **Connect Four** – gravity-based vertical grid; alignment win condition.
* **Snakes & Ladders** – linear board, dice-driven progression.
* **Ludo / Parcheesi** – race game with dice and safe zones.

---

## ♟️ Abstract Strategy

Abstract games are excellent for stress-testing **pattern-based movement** and deterministic rule evaluation.

* **Chess** (already implemented).
* **Go** – territory capture on grid; simple rules, deep complexity.
* **Reversi / Othello** – flipping mechanics, adjacency-based conditions.
* **Nine Men’s Morris** – placement + mill capture mechanic.

---

## 🎲 Dice & Movement Games

These exercise **dice artifacts, state transitions, and multi-phase turns**.

* **Backgammon** (already implemented).
* **Monopoly** – linear board loop, dice, property ownership, money tokens.
* **Clue / Cluedo** – dice-driven movement on a mansion graph; deduction layer.
* **Sorry!** – linear track with send-back captures.

---

## 🌍 Strategy / Territory Control

These push the engine toward **multi-graph boards, combat, and reinforcement mechanics**.

* **Risk** – world map, dice-based battles, reinforcement.
* **Axis & Allies** – territory control, economy, and combat system.
* **Stratego** – hidden piece ranks, capture/resolution mechanic.

---

## 🏞️ Resource Management & Trading

Here we enter **economy-driven games**, requiring custom artifacts like resources, tokens, and trade mechanics.

* **Settlers of Catan** – hex map, dice-based resource production, trading, building.
* **Carcassonne** – tile-laying, board expansion, meeple scoring.
* **Ticket to Ride** – graph of routes, card collection, longest path scoring.
* **Puerto Rico** – role selection, resource economy.

---

## 🃏 Card-Driven Hybrids

These challenge the engine with **deck-building, drafting, and evolving state mechanics**.

* **Dominion** – deck-building mechanic, evolving state.
* **Splendor** – chip-based economy + card engine-building.
* **7 Wonders** – drafting, adjacency scoring, multi-phase play.

---

## Why This Mix Matters

* **Simple linear movement** (*Snakes & Ladders*, *Ludo*) → validates turn sequencing and dice handling.
* **Abstract strategy** (*Checkers*, *Go*) → stress-test movement patterns and capture logic.
* **Resource & trading games** (*Catan*, *Carcassonne*) → stretch the engine into economy systems and non-spatial artifacts.
* **Territory control** (*Risk*, *Axis & Allies*) → test large multi-graph boards and multi-step resolution.
* **Card-driven hybrids** (*Dominion*, *Splendor*) → introduce deck/hand management and new state abstractions.

---

👉 **Next Step:** Prioritize implementation order. Start with **race/abstract games** (Ludo, Checkers, Monopoly), then advance to **Risk** and **Catan** for multi-phase and resource trading, before tackling **deck-builders** like Dominion.
