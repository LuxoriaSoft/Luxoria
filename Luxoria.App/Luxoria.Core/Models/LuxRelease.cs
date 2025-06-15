using Octokit;

namespace Luxoria.Core.Models;

public record LuxRelease
{
    public long Id { get; private set; }
    public string Name { get; init; }
    public string Body { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }
    public Author Author { get; private set; }

    public record LuxMod(ReleaseAsset asset)
    {
        public string Name { get; private set; } = asset.Name;
        public int DownloadCount { get; private set; } = asset.DownloadCount;
        public string BrowserDownloadUrl { get; private set; } = asset.BrowserDownloadUrl;
        public int Size { get; private set; } = asset.Size;
        public DateTimeOffset CreatedAt { get; private set; } = asset.CreatedAt;
        public DateTimeOffset UpdatedAt { get; private set; } = asset.UpdatedAt;
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