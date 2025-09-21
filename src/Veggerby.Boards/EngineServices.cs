using System;
using System.Collections.Generic;

namespace Veggerby.Boards;

/// <summary>
/// Internal service container attached to a <see cref="GameEngine"/> for experimental / feature-flagged subsystems
/// (compiled patterns, future bitboards). Public only to satisfy visibility requirements of the engine constructor
/// but not intended for external consumption.
/// </summary>
public sealed class EngineServices
{
    private readonly Dictionary<Type, object> _items = new();
    /// <summary>
    /// Gets an empty service container instance.
    /// </summary>
    public static EngineServices Empty { get; } = new EngineServices();

    /// <summary>
    /// Registers or replaces a service instance of type <typeparamref name="T"/>.
    /// </summary>
    public void Set<T>(T value) where T : class
    {
        _items[typeof(T)] = value;
    }

    /// <summary>
    /// Attempts to retrieve a previously registered service of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The resolved service or null.</param>
    /// <returns>True when found; otherwise false.</returns>
    public bool TryGet<T>(out T value) where T : class
    {
        if (_items.TryGetValue(typeof(T), out var v) && v is T cast)
        {
            value = cast; return true;
        }
        value = null; return false;
    }
}