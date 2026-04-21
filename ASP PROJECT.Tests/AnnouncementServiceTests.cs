using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASP_PROJECT.Tests;

public class AnnouncementServiceTests
{
    [Fact]
    public async Task GetAllAsync_SplitsPinnedAndRecent_AndOrdersByNewestFirst()
    {
        await using var dbContext = CreateDbContext(nameof(GetAllAsync_SplitsPinnedAndRecent_AndOrdersByNewestFirst));
        dbContext.Announcements.AddRange(
            new Announcement { Id = 1, Title = "Older pinned", Content = "Pinned content for ordering checks.", IsPinned = true, PublishedOnUtc = DateTime.UtcNow.AddDays(-3) },
            new Announcement { Id = 2, Title = "Newest pinned", Content = "Pinned content that should appear first.", IsPinned = true, PublishedOnUtc = DateTime.UtcNow.AddDays(-1) },
            new Announcement { Id = 3, Title = "Recent item", Content = "Recent content for visitors to read on the site.", IsPinned = false, PublishedOnUtc = DateTime.UtcNow.AddDays(-2) });
        await dbContext.SaveChangesAsync();

        var service = new AnnouncementService(dbContext);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Pinned.Count);
        Assert.Equal("Newest pinned", result.Pinned.First().Title);
        Assert.Single(result.Recent);
        Assert.Equal("Recent item", result.Recent.First().Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingAnnouncement()
    {
        await using var dbContext = CreateDbContext(nameof(GetByIdAsync_ReturnsMatchingAnnouncement));
        dbContext.Announcements.Add(new Announcement { Id = 7, Title = "Lookup", Content = "Announcement lookup content for the details page.", IsPinned = false });
        await dbContext.SaveChangesAsync();

        var service = new AnnouncementService(dbContext);

        var result = await service.GetByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal("Lookup", result!.Title);
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
