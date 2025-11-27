namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Defines the color groups for Monopoly properties.
/// </summary>
public enum PropertyColorGroup
{
    /// <summary>
    /// Not a colored property (utility, railroad, etc.).
    /// </summary>
    None,

    /// <summary>
    /// Brown properties (Mediterranean, Baltic).
    /// </summary>
    Brown,

    /// <summary>
    /// Light Blue properties (Oriental, Vermont, Connecticut).
    /// </summary>
    LightBlue,

    /// <summary>
    /// Pink properties (St. Charles, States, Virginia).
    /// </summary>
    Pink,

    /// <summary>
    /// Orange properties (St. James, Tennessee, New York).
    /// </summary>
    Orange,

    /// <summary>
    /// Red properties (Kentucky, Indiana, Illinois).
    /// </summary>
    Red,

    /// <summary>
    /// Yellow properties (Atlantic, Ventnor, Marvin Gardens).
    /// </summary>
    Yellow,

    /// <summary>
    /// Green properties (Pacific, North Carolina, Pennsylvania).
    /// </summary>
    Green,

    /// <summary>
    /// Dark Blue properties (Park Place, Boardwalk).
    /// </summary>
    DarkBlue,

    /// <summary>
    /// Railroad (not a color group, but used for railroad properties).
    /// </summary>
    Railroad,

    /// <summary>
    /// Utility (not a color group, but used for utility properties).
    /// </summary>
    Utility
}
