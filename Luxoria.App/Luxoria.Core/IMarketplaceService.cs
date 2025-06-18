using Luxoria.Core.Models;

namespace Luxoria.Core.Interfaces;

public interface IMarketplaceService
{
    Task<ICollection<LuxRelease>> GetReleases();
    Task<ICollection<LuxRelease.LuxMod>> GetRelease(long releaseId);
}
