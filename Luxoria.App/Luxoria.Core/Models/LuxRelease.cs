using Octokit;
using System.Runtime.InteropServices;

namespace Luxoria.Core.Models;

public record LuxRelease
{
    public long Id { get; private set; }
    public string Name { get; init; }
    public string Body { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }
    public Author Author { get; private set; }

    public record LuxMod(string Name, int DownloadCount, string DownloadUrl, ICollection<LuxMod> AttachedModulesByArch)
    {
        public string Name { get; set; } = Name;

        public int DownloadCount { get; set; } = DownloadCount;

        public string DownloadUrl { get; set; } = DownloadUrl;

        public ICollection<LuxMod> AttachedModules = AttachedModulesByArch;
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