using Luxoria.Core.Services;

namespace Luxoria.Core.Interfaces;

public interface ICoreUpdaterService
{
    Task InstallAllUpdatesAsync(IEnumerable<IUpdater> updaters);
}