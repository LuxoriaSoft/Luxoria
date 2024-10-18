using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models.Events
{
    public class TextInputEvent
    {
        public string Text { get; }

        public TextInputEvent(string text)
        {
            Text = text;
        }
    }
}
