using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace HistoryDemo;

/// <summary>
/// Demonstrates the history navigation and branching capabilities of Veggerby.Boards.
/// Shows:
/// 1. Linear history with undo/redo
/// 2. Timeline navigation (GoTo)
/// 3. Branching timelines for "what-if" analysis
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Veggerby.Boards - History/Undo/Redo Demonstration          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        DemoLinearHistory();
        Console.WriteLine();

        DemoTimelineNavigation();
        Console.WriteLine();

        DemoBranchingHistory();
    }

    /// <summary>
    /// Demonstrates basic linear history with undo and redo operations.
    /// </summary>
    static void DemoLinearHistory()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("DEMO 1: Linear History with Undo/Redo");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        // Initialize chess game
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var history = new GameHistory(progress);

        Console.WriteLine($"Initial state: Index {history.CurrentIndex}/{history.Length - 1}");
        Console.WriteLine($"Can Undo: {history.CanUndo}, Can Redo: {history.CanRedo}");
        Console.WriteLine();

        // Apply some moves
        Console.WriteLine("▶ Playing moves: e4, e5, Nf3, Nc6");
        history = ApplyMove(history, "e4");
        history = ApplyMove(history, "e5");
        history = ApplyMove(history, "Nf3");
        history = ApplyMove(history, "Nc6");

        Console.WriteLine($"After 4 moves: Index {history.CurrentIndex}/{history.Length - 1}");
        Console.WriteLine($"Can Undo: {history.CanUndo}, Can Redo: {history.CanRedo}");
        Console.WriteLine();

        // Undo last two moves
        Console.WriteLine("⏪ Undoing last two moves...");
        history = history.Undo();
        Console.WriteLine($"  After undo 1: Index {history.CurrentIndex}/{history.Length - 1}");
        history = history.Undo();
        Console.WriteLine($"  After undo 2: Index {history.CurrentIndex}/{history.Length - 1}");
        Console.WriteLine($"Can Undo: {history.CanUndo}, Can Redo: {history.CanRedo}");
        Console.WriteLine();

        // Redo one move
        Console.WriteLine("⏩ Redoing one move...");
        history = history.Redo();
        Console.WriteLine($"  After redo: Index {history.CurrentIndex}/{history.Length - 1}");
        Console.WriteLine($"Can Undo: {history.CanUndo}, Can Redo: {history.CanRedo}");
        Console.WriteLine();

        // Apply alternative move (truncates future)
        Console.WriteLine("▶ Playing alternative move Bc4 (truncates future timeline)");
        history = ApplyMove(history, "Bc4");
        Console.WriteLine($"After alternative: Index {history.CurrentIndex}/{history.Length - 1}");
        Console.WriteLine($"Can Redo: {history.CanRedo} (future timeline was truncated)");
    }

    /// <summary>
    /// Demonstrates direct timeline navigation using GoTo.
    /// </summary>
    static void DemoTimelineNavigation()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("DEMO 2: Timeline Navigation with GoTo");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var history = new GameHistory(progress);

        // Play a sequence of moves
        Console.WriteLine("▶ Playing opening sequence: 1.e4 e5 2.Nf3 Nc6 3.Bc4 Nf6");
        history = ApplyMove(history, "e4");
        history = ApplyMove(history, "e5");
        history = ApplyMove(history, "Nf3");
        history = ApplyMove(history, "Nc6");
        history = ApplyMove(history, "Bc4");
        history = ApplyMove(history, "Nf6");

        Console.WriteLine($"Timeline length: {history.Length} positions");
        Console.WriteLine();

        // Navigate to different points
        Console.WriteLine("🔍 Navigating to position 0 (start)...");
        var atStart = history.GoTo(0);
        Console.WriteLine($"  Position {atStart.CurrentIndex}: Initial position");

        Console.WriteLine("🔍 Navigating to position 3 (after 1...Nc6)...");
        var atMove3 = history.GoTo(4);
        Console.WriteLine($"  Position {atMove3.CurrentIndex}: After black's Nc6");

        Console.WriteLine("🔍 Navigating to position 6 (current)...");
        var atCurrent = history.GoTo(6);
        Console.WriteLine($"  Position {atCurrent.CurrentIndex}: Current position");
        Console.WriteLine();

        // Show event history at different points
        Console.WriteLine("📜 Event history at position 3:");
        var events = atMove3.GetEventHistory();
        Console.WriteLine($"   Total events: {events.Count}");
    }

    /// <summary>
    /// Demonstrates branching timelines for scenario exploration.
    /// </summary>
    static void DemoBranchingHistory()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("DEMO 3: Branching History for 'What-If' Analysis");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var history = new BranchingGameHistory(progress);

        Console.WriteLine("▶ Main line: 1.e4 e5 2.Nf3");
        history = ApplyBranchingMove(history, "e4");
        history = ApplyBranchingMove(history, "e5");
        history = ApplyBranchingMove(history, "Nf3");

        var mainBranchId = history.CurrentBranch.Id;
        Console.WriteLine($"Main branch '{history.CurrentBranch.Name}' at position {history.CurrentBranch.CurrentIndex}");
        Console.WriteLine();

        // Create alternative branch
        Console.WriteLine("🌿 Creating alternative branch 'Italian Game'...");
        history = history.Undo();
        Console.WriteLine($"Moved back to position {history.CurrentBranch.CurrentIndex}");
        history = history.CreateBranch("Italian Game");
        Console.WriteLine($"Created branch '{history.CurrentBranch.Name}'");

        Console.WriteLine("▶ Alternative: 2.Bc4 Nc6");
        history = ApplyBranchingMove(history, "Bc4");
        history = ApplyBranchingMove(history, "Nc6");
        Console.WriteLine($"Alternative branch at position {history.CurrentBranch.CurrentIndex}");
        Console.WriteLine();

        // Show branch information
        Console.WriteLine($"📊 Total branches: {history.Branches.Count}");
        foreach (var branch in history.Branches.Values)
        {
            Console.WriteLine($"   - {branch.Name} (ID: {branch.Id}): {branch.Length} positions");
            if (branch.ParentBranchId != null)
            {
                Console.WriteLine($"     Forked from {branch.ParentBranchId} at position {branch.BranchPointIndex}");
            }
        }
        Console.WriteLine();

        // Switch back to main branch
        Console.WriteLine($"🔄 Switching back to main branch...");
        history = history.SwitchToBranch(mainBranchId);
        Console.WriteLine($"Now on branch '{history.CurrentBranch.Name}' at position {history.CurrentBranch.CurrentIndex}");

        // Continue main line
        Console.WriteLine("▶ Continuing main line: 2...Nc6");
        history = ApplyBranchingMove(history, "Nc6");
        Console.WriteLine($"Main branch now at position {history.CurrentBranch.CurrentIndex}");
        Console.WriteLine();

        // Compare branches
        Console.WriteLine("📊 Branch comparison:");
        foreach (var branch in history.Branches.Values)
        {
            var branchHistory = history.SwitchToBranch(branch.Id);
            var events = branchHistory.GetEventHistory();
            Console.WriteLine($"   {branch.Name}: {events.Count} events, position {branch.CurrentIndex}");
        }
    }

    /// <summary>
    /// Helper method to apply a chess move using SAN notation.
    /// </summary>
    static GameHistory ApplyMove(GameHistory history, string san)
    {
        try
        {
            var newProgress = history.Current.MoveSan(san);
            var events = newProgress.Events.Skip(history.Current.Events.Count()).ToList();
            if (events.Any())
            {
                return history.Apply(events.First());
            }

            Console.WriteLine($"  ⚠️  No event generated for move {san}");
            return history;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Error applying move {san}: {ex.Message}");
            return history;
        }
    }

    /// <summary>
    /// Helper method to apply a chess move to branching history.
    /// </summary>
    static BranchingGameHistory ApplyBranchingMove(BranchingGameHistory history, string san)
    {
        try
        {
            var newProgress = history.Current.MoveSan(san);
            var events = newProgress.Events.Skip(history.Current.Events.Count()).ToList();
            if (events.Any())
            {
                return history.Apply(events.First());
            }

            Console.WriteLine($"  ⚠️  No event generated for move {san}");
            return history;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Error applying move {san}: {ex.Message}");
            return history;
        }
    }
}
