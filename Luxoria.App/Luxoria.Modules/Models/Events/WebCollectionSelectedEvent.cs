using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;


[ExcludeFromCodeCoverage]
public class WebCollectionSelectedEvent : IEvent
{
    public Guid CollectionId { get; private set; }
    public WebCollectionSelectedEvent(Guid collectionId)
    {
        CollectionId = collectionId;
    }
}