using Luxoria.Core.Interfaces;
using Luxoria.Core.Models;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Octokit;

namespace Luxoria.Core.Services;

public class MarketplaceService(ILoggerService logger, string owner = "luxoriasoft", string repository = "marketplace") : IMarketplaceService
{
    private readonly string _owner = owner;
    private readonly string _repository = repository;
    private readonly ILoggerService _logger = logger;
    private readonly GitHubClient _client = new (new ProductHeaderValue("Luxoria.Core"));

    public async Task<ICollection<LuxRelease>> GetReleases()
    {
        _logger.Log($"Fetching releases from {_owner}/{_repository}...");
        IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll(_owner, _repository);
        _logger.Log($"Found {releases.Count} releases.");

        return [.. releases.Select(x => new LuxRelease(x))];
    }

    public async Task<ICollection<LuxRelease.LuxMod>> GetRelease(long releaseId)
    {
        _logger.Log($"Fetching assets for release ID {releaseId} from {_owner}/{_repository}...");
        Release githubRelease = await _client.Repository.Release.Get(_owner, _repository, releaseId);
        IReadOnlyList<ReleaseAsset> assets = githubRelease.Assets;
        _logger.Log($"Found {assets.Count} assets for release ID {releaseId}.");

        return [.. assets.Select(x => new LuxRelease.LuxMod(x))];
    }
}