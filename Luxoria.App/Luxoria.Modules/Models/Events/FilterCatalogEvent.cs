using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class FilterCatalogEvent : IEvent
{
    /// <summary>
    /// TaskCompletionSource to store the response from subscribers
    /// </summary>
    public TaskCompletionSource<List<(string Name, string Description, string Version)>> Response { get; }

    public FilterCatalogEvent()
    {
        Response = new TaskCompletionSource<List<(string Name, string Description, string Version)>>();
    }
}
