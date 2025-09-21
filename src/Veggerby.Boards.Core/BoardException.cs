using System;
using System.Diagnostics.CodeAnalysis;

namespace Veggerby.Boards.Core;

/// <summary>
/// Domain exception representing invariant violations or invalid board operations.
/// </summary>
[ExcludeFromCodeCoverage]
public class BoardException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoardException"/> class.
    /// </summary>
    public BoardException() { }
    /// <summary>
    /// Initializes a new instance with an error message.
    /// </summary>
    public BoardException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance with an error message and inner exception.
    /// </summary>
    public BoardException(string message, System.Exception inner) : base(message, inner) { }
}