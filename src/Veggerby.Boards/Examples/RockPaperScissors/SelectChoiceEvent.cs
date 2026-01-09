using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Event representing a player selecting their choice in Rock-Paper-Scissors.
/// </summary>
/// <param name="Player">The player making the selection.</param>
/// <param name="Choice">The choice being made (Rock, Paper, or Scissors).</param>
public sealed record SelectChoiceEvent(Player Player, Choice Choice) : IGameEvent
{
}
