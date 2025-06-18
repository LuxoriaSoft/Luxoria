using Newtonsoft.Json;
using Octokit;

namespace Luxoria.Core.Models;

/// <summary>
/// LuxRelease, used to represent a Github Release
/// </summary>
public record LuxRelease
{
    /// <summary>
    /// Github Release ID
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// Github Release Name
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Github description of the release
    /// </summary>
    public string Body { get; init; }
    /// <summary>
    /// Created at date of the release
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// Published at date of the release
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }
    /// <summary>
    /// Author processing the release
    /// </summary>
    public Author Author { get; init; }

    /// <summary>
    /// Assets attached to the release
    /// </summary>
    /// <param name="Name">Asset name</param>
    /// <param name="DownloadCount">Download count</param>
    /// <param name="DownloadUrl">Download Url</param>
    /// <param name="AttachedModulesByArch">Asset derivated for each architecture (x64, x86, arm64)</param>
    public record LuxMod(string Name, int DownloadCount, string DownloadUrl, ICollection<LuxMod> AttachedModulesByArch)
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Name { get; set; } = Name;

        /// <summary>
        /// Download count of the asset
        /// </summary>
        public int DownloadCount { get; set; } = DownloadCount;

        /// <summary>
        /// Download URL of the asset
        /// </summary>
        public string DownloadUrl { get; set; } = DownloadUrl;

        /// <summary>
        /// Derived modules attached to this asset, by architecture
        /// </summary>
        public ICollection<LuxMod> AttachedModules = AttachedModulesByArch;

        /// <summary>
        /// Attached modules download count
        /// </summary>
        public int AttachedModulesDownloadCount => AttachedModules.Sum(x => x.DownloadCount);
    }

    /// <summary>
    /// Constructor Cache Usage
    /// </summary>
    [JsonConstructor]
    public LuxRelease(long id, string name, string body, DateTimeOffset createdAt, DateTimeOffset? publishedAt, Author author)
    {
        Id = id;
        Name = name;
        Body = body;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
        Author = author;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public LuxRelease(Release release)
    {
        Id = release.Id;
        Name = release.Name;
        Body = release.Body;
        CreatedAt = release.CreatedAt;
        PublishedAt = release.PublishedAt;
        Author = release.Author;
    }
}