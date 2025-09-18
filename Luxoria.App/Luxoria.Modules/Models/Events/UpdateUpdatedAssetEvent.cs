using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class UpdateUpdatedAssetEvent : IEvent
{
    public Guid AssetId { get; private set; }
    public string Url { get; private set; }
    public Guid CollectionId { get; private set; }
    public Guid LastUploadedId { get; private set; }


    public UpdateUpdatedAssetEvent(Guid assetId, string url, Guid collectionId, Guid lastUploadedId)
    {
        AssetId = assetId;
        Url = url;
        CollectionId = collectionId;
        LastUploadedId = lastUploadedId;
    }
}