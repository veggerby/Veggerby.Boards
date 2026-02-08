# FIBS Rating System

## Overview

The FIBS (First Internet Backgammon Server) Rating System is a standardized method for tracking player skill levels in backgammon. This implementation provides deterministic, immutable rating calculations integrated with the Veggerby.Boards game engine architecture.

## Background

The FIBS rating system was developed for online backgammon and has become a widely-used standard. It calculates rating changes based on:

1. **Match Length** - Longer matches have proportionally greater rating impact
2. **Player Experience** - New players have more volatile ratings
3. **Rating Differential** - Win probability is calculated from the rating difference

## Key Concepts

### Rating

- **Initial Rating**: 1500 for new players
- **Typical Range**: 1000-2000
- **Updates**: Only after match completion (not per-game)

### Experience

- **Definition**: Cumulative match length played
- **Initial Value**: 0
- **Volatility Threshold**: Players with experience < 400 have increased rating volatility
- **Factor Formula**: `PE = max(1, 5 - ((E + n) / 100))`
  - Where `E` is current experience and `n` is match length
  - Results in volatility factor between 1.0 and 5.0

### Match Length

- **Definition**: Agreed-upon target score before match starts
- **Common Values**: 1, 3, 5, 7, 9, 11, 13 points
- **Not Final Score**: Match length is the pre-agreed target, not the actual final score
- **Scaling**: Rating impact scales with √n (square root of match length)

## FIBS Formula

### Variables

- `n` = match length (agreed upon, not final score)
- `P1`, `P2` = player ratings
- `E1`, `E2` = player experience before match
- `D` = |P1 - P2| (absolute rating difference)
- `√n` = square root of match length

### Win Probability

- **Underdog Win Probability**: `U = 1 / (10^(D×√n/2000) + 1)`
- **Favorite Win Probability**: `F = 1 - U`

### Experience Factor

- **Formula**: `PE = max(1, 5 - ((E + n) / 100))`
- **Purpose**: Increases rating volatility for inexperienced players
- **Range**: 1.0 (experienced) to ~5.0 (brand new player)
- **Threshold**: Volatility > 1.0 when experience < 400

### Rating Change

```
Rating Change = ±4 × PE × √n × Probability
```

Where:
- **Winner (higher rated)**: +4 × PE × √n × U
- **Winner (lower rated)**: +4 × PE × √n × F
- **Loser (higher rated)**: -4 × PE × √n × F
- **Loser (lower rated)**: -4 × PE × √n × U

## Usage

### Initialization

Initialize FIBS ratings on a game progress instance:

```csharp
var progress = new BackgammonGameBuilder().Compile();

// With default ratings (1500) and experience (0)
progress = progress.WithFibsRatings(matchLength: 5);

// With custom initial ratings
progress = progress.WithFibsRatings(
    matchLength: 7,
    whiteRating: 1600.0,
    whiteExperience: 150,
    blackRating: 1450.0,
    blackExperience: 200);

// Unlimited match (no rating updates)
progress = progress.WithFibsRatings(
    matchLength: 0, 
    isUnlimited: true);
```

### Querying Ratings

```csharp
// Get a player's current rating
var whiteRating = progress.GetFibsRating("white");
if (whiteRating != null)
{
    Console.WriteLine($"White: {whiteRating.Rating:F1} " +
                      $"(Experience: {whiteRating.Experience})");
}

// Get match configuration
var config = progress.GetFibsMatchConfig();
```

### Automatic Updates

Ratings are automatically updated when a game ends:

```csharp
// Play the game normally
progress = progress.HandleEvent(...);

// When game ends, ratings are automatically updated
if (progress.IsGameOver())
{
    var whiteRating = progress.GetFibsRating("white");
    var blackRating = progress.GetFibsRating("black");
    
    Console.WriteLine($"Final Ratings:");
    Console.WriteLine($"  White: {whiteRating.Rating:F1}");
    Console.WriteLine($"  Black: {blackRating.Rating:F1}");
}
```

## Examples

### Equal Players, Single Point Match

```csharp
// Both players at 1500, no experience, 1-point match
// Winner: +9.98 → 1509.98 (exp: 1)
// Loser:  -9.98 → 1490.02 (exp: 1)
```

### Upset Victory

```csharp
// Underdog (1400) defeats favorite (1600)
// Winner: +16.16 → 1416.16 (larger gain)
// Loser:  -8.84  → 1591.16 (smaller loss)
```

### Longer Match (9 Points)

```csharp
// Equal players, 9-point match
// Winner: +29.46 → 1529.46 (exp: 9)
// Loser:  -29.46 → 1470.54 (exp: 9)
// Note: 3x the change of 1-point (√9 = 3)
```

## References

- [FIBS Rating System](https://bkgm.com/articles/McCool/ratings.html) - David McCool's comprehensive explanation
- Match length: square root scaling factor
- Experience thresholds: 0-400 range with volatility factor
- Rating range: typical 1000-2000, new players start at 1500
