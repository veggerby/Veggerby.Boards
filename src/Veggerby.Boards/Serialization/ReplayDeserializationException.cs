using System;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Exception thrown when deserialization of a replay envelope fails.
/// </summary>
public class ReplayDeserializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayDeserializationException"/> class.
    /// </summary>
    public ReplayDeserializationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayDeserializationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ReplayDeserializationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayDeserializationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ReplayDeserializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
