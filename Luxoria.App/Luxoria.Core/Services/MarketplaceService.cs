using Luxoria.Core.Interfaces;
using Luxoria.Core.Models;
using Luxoria.SDK.Interfaces;
using Octokit;

namespace Luxoria.Core.Services;

/// <summary>
/// Marketplace Service
/// </summary>
/// <param name="logger">ILoggerService retreived from Services</param>
/// <param name="owner">Github Organisation/User (default LuxoriaSoft), check it out at : https://github.com/LuxoriaSoft</param>
/// <param name="repository">Github repository (default marketplace), have a look at : https://github.com/LuxoriaSoft/marketplace</param>
public class MarketplaceService(ILoggerService logger, string owner = "luxoriasoft", string repository = "marketplace") : IMarketplaceService
{
    /// <summary>
    /// Repository owner, default is "luxoriasoft"
    /// </summary>
    private readonly string _owner = owner;
    /// <summary>
    /// Repository name, default is "marketplace"
    /// </summary>
    private readonly string _repository = repository;
    /// <summary>
    /// Logger service to log messages
    /// </summary>
    private readonly ILoggerService _logger = logger;
    /// <summary>
    /// Github client used to fetch releases and assets
    /// </summary>
    private readonly GitHubClient _client = new(new ProductHeaderValue("Luxoria.Core"));

    /// <summary>
    /// Gets releases from marketplace repository
    /// </summary>
    /// <returns>Returns an collection of releases [v1.1, v1.2, etc.]</returns>
    public async Task<ICollection<LuxRelease>> GetReleases()
    {
        _logger.Log($"Fetching releases from {_owner}/{_repository}...");
        IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll(_owner, _repository);
        _logger.Log($"Found {releases.Count} releases.");

        return [.. releases.Select(x => new LuxRelease(x))];
    }

    /// <summary>
    /// Gets a specific release by ID
    /// </summary>
    /// <param name="releaseId">Github release Id (defined as long)</param>
    /// <returns>Returns a collection of artifacts</returns>
    public async Task<ICollection<LuxRelease.LuxMod>> GetRelease(long releaseId)
    {
        _logger.Log($"Fetching assets for release ID {releaseId} from {_owner}/{_repository}...");
        Release githubRelease = await _client.Repository.Release.Get(_owner, _repository, releaseId);
        IReadOnlyList<ReleaseAsset> assets = githubRelease.Assets;
        _logger.Log($"Found {assets.Count} assets for release ID {releaseId}.");

        ICollection<(string Name, string DownloadUrl)> keys = assets
            .Where(x => x.Name.EndsWith(".readme.md"))
            .Select(x => (x.Name.Replace(".readme.md", ""), x.BrowserDownloadUrl))
            .ToList();

        _logger.Log($"Found {keys.Count} modules with README files.");

        List<LuxRelease.LuxMod> modules = [];

        foreach ((string Name, string DownloadUrl) key in keys)
        {
            var attachedModules = assets
                .Where(x => x.Name.StartsWith(key.Name) && !x.Name.EndsWith(".readme.md"))
                .Select(x => new LuxRelease.LuxMod(
                    Name: x.Name,
                    DownloadCount: x.DownloadCount,
                    DownloadUrl: x.BrowserDownloadUrl,
                    AttachedModulesByArch: new List<LuxRelease.LuxMod>()))
                .ToList();
            modules.Add(new(key.Name, 0, key.DownloadUrl, attachedModules));
        }

        return modules;
    }
}