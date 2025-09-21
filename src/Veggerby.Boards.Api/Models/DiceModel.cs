namespace Veggerby.Boards.Api.Models;

/// <summary>
/// DTO representing dice state.
/// </summary>
public class DiceModel
{
    /// <summary>
    /// Gets or sets the dice identifier.
    /// </summary>
    public string DiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public int CurrentValue { get; set; }
}