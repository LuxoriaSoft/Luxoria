using Newtonsoft.Json;
using Octokit;

namespace Luxoria.Core.Models;

public record LuxRelease
{
    public long Id { get; init; }
    public string Name { get; init; }
    public string Body { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public Author Author { get; init; }

    public record LuxMod(string Name, int DownloadCount, string DownloadUrl, ICollection<LuxMod> AttachedModulesByArch)
    {
        public string Name { get; set; } = Name;

        public int DownloadCount { get; set; } = DownloadCount;

        public string DownloadUrl { get; set; } = DownloadUrl;

        public ICollection<LuxMod> AttachedModules = AttachedModulesByArch;
    }

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