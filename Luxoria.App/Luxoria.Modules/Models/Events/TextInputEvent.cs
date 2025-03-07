using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event triggered when text input is received.
/// </summary>
[ExcludeFromCodeCoverage]
public class TextInputEvent : IEvent
{
    /// <summary>
    /// Gets the input text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputEvent"/> class.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> is empty.</exception>
    public TextInputEvent(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text), "Text cannot be null.");
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty or whitespace.", nameof(text));
        }

        Text = text;
    }
}
