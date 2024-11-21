using System;

namespace Luxoria.Modules.Models.Events
{
    public class TextInputEvent
    {
        public string Text { get; }

        public TextInputEvent(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Text cannot be null.");
            }
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            }

            Text = text;
        }
    }
}