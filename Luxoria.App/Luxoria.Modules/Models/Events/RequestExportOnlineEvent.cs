using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class RequestExportOnlineEvent : IEvent
{
    public string AssetPath{ get; init; }
    public LuxAsset Asset { get; init; }

    public RequestExportOnlineEvent(string path, LuxAsset asset)
    {
        AssetPath = path;
        Asset = asset;
    }
}
