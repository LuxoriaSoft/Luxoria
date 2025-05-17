using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event triggered when a collection is updated.
/// Contains information about the updated collection, including its name, path, and assets.
/// </summary>
[ExcludeFromCodeCoverage]
public class CollectionUpdatedEvent : IEvent
{
    /// <summary>
    /// Gets the name of the updated collection.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Gets the file path associated with the collection.
    /// </summary>
    public string CollectionPath { get; }

    /// <summary>
    /// Gets the collection of assets that are part of this update.
    /// </summary>
    public ICollection<LuxAsset> Assets { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionUpdatedEvent"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="collectionPath">The file path associated with the collection.</param>
    /// <param name="assets">The assets contained within the collection.</param>
    public CollectionUpdatedEvent(string collectionName, string collectionPath, ICollection<LuxAsset> assets)
    {
        CollectionName = collectionName;
        CollectionPath = collectionPath;
        Assets = assets;
    }
}
