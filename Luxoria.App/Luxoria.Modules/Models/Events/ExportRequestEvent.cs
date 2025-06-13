using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class ExportRequestEvent: IEvent
{
    public required ICollection<LuxAsset> Assets { get; set; }
}
