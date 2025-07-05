using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class RequestTokenEvent(Action<string> onHandleReceived) : IEvent
{
    public Action<string>? OnHandleReceived { get; } = onHandleReceived;
}
