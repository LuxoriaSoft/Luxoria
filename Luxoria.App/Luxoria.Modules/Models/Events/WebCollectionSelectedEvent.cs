using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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