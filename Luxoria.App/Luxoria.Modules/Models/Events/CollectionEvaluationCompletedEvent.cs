using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event triggered when LuxFilter has done and computed every asset.
/// Will contain a list of LuxAsset.Id with a dictionary of scores for each asset.
/// </summary>
[ExcludeFromCodeCoverage]
public class CollectionEvaluationCompletedEvent : IEvent
{
    /// <summary>
    /// Contains the scores for each asset in the collection.
    /// </summary>
    public Dictionary<Guid, double> AssetsScores { get; set; } = [];

    /// <summary>
    /// Computed at
    /// </summary>
    public DateTime ComputedAt { get; } = DateTime.UtcNow;
}
