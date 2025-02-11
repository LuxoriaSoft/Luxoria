using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models.Events
{
    [ExcludeFromCodeCoverage]
    public class RequestWindowHandleEvent : IEvent
    {
        public Action<nint>? OnHandleReceived { get; }

        public RequestWindowHandleEvent(Action<nint> onHandleReceived)
        {
            OnHandleReceived = onHandleReceived;
        }
    }
}
