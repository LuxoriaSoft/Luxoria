using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class SaveLastUpdatedIdEvent : IEvent
{
    public string Url { get; private set; }
    public Guid LastUpdatedId { get; private set; }
    public Guid CollectionId { get; private set; }
    public Guid LuxAssetId { get; private set; }
    public SaveLastUpdatedIdEvent(string url, Guid lastUpdatedId, Guid collectionId, Guid luxAssetId)
    {
        Url = url;
        LastUpdatedId = lastUpdatedId;
        CollectionId = collectionId;
        LuxAssetId = luxAssetId;
    }
}
