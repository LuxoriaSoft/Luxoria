
using Luxoria.Core.Helpers;
using Luxoria.Core.Interfaces;

namespace Luxoria.Core.Services;

public interface IUpdater
{
    Version CurrentVersion { get; }

    IUpdater SetCurrentVersion(Version currentVersion);

    /// <summary>
    /// Check whether an update is available, returns next version if available
    /// </summary>
    /// <returns>Returns next version if update is out-to-dated, null otherwise</returns>
    #nullable enable
    Task<Version?> CheckForUpdateAsync();
    #nullable disable

    Task<bool> UpdateAsync();
}

public interface IUpdateCachingMecanism
{
    byte[] Checksum { get; }
    DateTime LastCheckout { get; }
    TimeOnly IntervalBetweenRenewal { get; }
    bool IsExpired { get; }
}

public class CoreUpdaterService : ICoreUpdaterService
{
    /// <summary>
    /// CoreUpdaterService Constructor
    /// </summary>
    public CoreUpdaterService() { }

    /// <summary>
    /// Install all updates on background task
    /// </summary>
    /// <param name="updaters">Updaters known as IUpdater</param>
    public async Task InstallAllUpdatesAsync(IEnumerable<IUpdater> updaters)
    {
        List<Task> tasks = [];
        foreach (var updater in updaters)
        {
            tasks.Add(Task.Run(async () =>
            {
                var nVersion = await updater.CheckForUpdateAsync();
                if (nVersion is not null)
                {
                    await updater.UpdateAsync();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }
}
