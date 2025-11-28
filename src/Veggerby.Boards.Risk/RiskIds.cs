namespace Veggerby.Boards.Risk;

/// <summary>
/// Provides constant identifiers for Risk game artifacts.
/// </summary>
public static class RiskIds
{
    /// <summary>
    /// Player identifiers.
    /// </summary>
    public static class Players
    {
        /// <summary>Red player.</summary>
        public const string Red = "player-red";

        /// <summary>Blue player.</summary>
        public const string Blue = "player-blue";

        /// <summary>Green player (for 3+ player games).</summary>
        public const string Green = "player-green";

        /// <summary>Yellow player (for 4+ player games).</summary>
        public const string Yellow = "player-yellow";
    }

    /// <summary>
    /// Continent identifiers.
    /// </summary>
    public static class Continents
    {
        /// <summary>North America continent.</summary>
        public const string North = "continent-north";

        /// <summary>South America continent.</summary>
        public const string South = "continent-south";

        /// <summary>Europe continent.</summary>
        public const string Europe = "continent-europe";

        /// <summary>Asia continent.</summary>
        public const string Asia = "continent-asia";
    }

    /// <summary>
    /// Territory identifiers.
    /// </summary>
    public static class Territories
    {
        // North America
        /// <summary>Alaska territory.</summary>
        public const string Alaska = "territory-alaska";

        /// <summary>Northwest Territory.</summary>
        public const string NorthWest = "territory-northwest";

        /// <summary>Greenland territory.</summary>
        public const string GreenLand = "territory-greenland";

        /// <summary>Alberta territory.</summary>
        public const string Alberta = "territory-alberta";

        /// <summary>Ontario territory.</summary>
        public const string Ontario = "territory-ontario";

        /// <summary>Quebec territory.</summary>
        public const string Quebec = "territory-quebec";

        // South America
        /// <summary>Venezuela territory.</summary>
        public const string Venezuela = "territory-venezuela";

        /// <summary>Peru territory.</summary>
        public const string Peru = "territory-peru";

        /// <summary>Brazil territory.</summary>
        public const string Brazil = "territory-brazil";

        /// <summary>Argentina territory.</summary>
        public const string Argentina = "territory-argentina";

        // Europe
        /// <summary>Iceland territory.</summary>
        public const string Iceland = "territory-iceland";

        /// <summary>Great Britain territory.</summary>
        public const string Britain = "territory-britain";

        /// <summary>Scandinavia territory.</summary>
        public const string Scandinavia = "territory-scandinavia";

        /// <summary>Western Europe territory.</summary>
        public const string WestEurope = "territory-west-europe";

        /// <summary>Northern Europe territory.</summary>
        public const string NorthEurope = "territory-north-europe";

        /// <summary>Southern Europe territory.</summary>
        public const string SouthEurope = "territory-south-europe";

        /// <summary>Ukraine territory.</summary>
        public const string Ukraine = "territory-ukraine";

        // Asia
        /// <summary>Ural territory.</summary>
        public const string Ural = "territory-ural";

        /// <summary>Siberia territory.</summary>
        public const string Siberia = "territory-siberia";

        /// <summary>Mongolia territory.</summary>
        public const string Mongolia = "territory-mongolia";

        /// <summary>China territory.</summary>
        public const string China = "territory-china";

        /// <summary>India territory.</summary>
        public const string India = "territory-india";

        /// <summary>Middle East territory.</summary>
        public const string MiddleEast = "territory-middle-east";

        /// <summary>Japan territory.</summary>
        public const string Japan = "territory-japan";
    }
}
