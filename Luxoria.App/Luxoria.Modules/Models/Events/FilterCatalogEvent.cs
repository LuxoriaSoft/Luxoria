using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event used to request the catalog of available filters.
/// Subscribers should respond with a list of filters containing their name, description, and version.
/// </summary>
[ExcludeFromCodeCoverage]
public class FilterCatalogEvent : IEvent
{
    /// <summary>
    /// TaskCompletionSource to store the response from subscribers.
    /// This is used to asynchronously wait for the list of filters.
    /// </summary>
    public TaskCompletionSource<List<(string Name, string Description, string Version)>> Response { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterCatalogEvent"/> class.
    /// </summary>
    public FilterCatalogEvent()
    {
        Response = new TaskCompletionSource<List<(string Name, string Description, string Version)>>();
    }
}
