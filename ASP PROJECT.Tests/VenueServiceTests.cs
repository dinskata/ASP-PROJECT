using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASP_PROJECT.Tests;

public class VenueServiceTests
{
    [Fact]
    public async Task GetPagedAsync_FiltersBySearchTerm_AndCalculatesTotalPages()
    {
        await using var dbContext = CreateDbContext(nameof(GetPagedAsync_FiltersBySearchTerm_AndCalculatesTotalPages));
        dbContext.Venues.AddRange(
            new Venue { Id = 1, Name = "Alpha Hall", City = "Sofia", Address = "One", Capacity = 100, Description = "A venue" },
            new Venue { Id = 2, Name = "Beta Space", City = "Varna", Address = "Two", Capacity = 120, Description = "B venue" },
            new Venue { Id = 3, Name = "Gamma Hub", City = "Sofia", Address = "Three", Capacity = 140, Description = "C venue" });
        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var result = await service.GetPagedAsync("sofia", 1, 1);

        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetDetailsAsync_ReturnsOnlyPublishedUpcomingEvents()
    {
        await using var dbContext = CreateDbContext(nameof(GetDetailsAsync_ReturnsOnlyPublishedUpcomingEvents));
        dbContext.Categories.Add(new Category { Id = 1, Name = "Technology", Description = "Category" });
        dbContext.Venues.Add(new Venue { Id = 10, Name = "North Hall", City = "Ruse", Address = "Address", Capacity = 200, Description = "Venue description" });
        dbContext.Events.AddRange(
            new Event
            {
                Id = 1,
                Title = "Published event",
                Description = "A valid published event description with enough characters.",
                CategoryId = 1,
                VenueId = 10,
                StartsAtUtc = DateTime.UtcNow.AddDays(2),
                DurationMinutes = 120,
                Price = 20,
                SeatsAvailable = 50,
                IsPublished = true
            },
            new Event
            {
                Id = 2,
                Title = "Hidden draft",
                Description = "A valid unpublished event description with enough characters.",
                CategoryId = 1,
                VenueId = 10,
                StartsAtUtc = DateTime.UtcNow.AddDays(3),
                DurationMinutes = 120,
                Price = 20,
                SeatsAvailable = 50,
                IsPublished = false
            });
        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var result = await service.GetDetailsAsync(10);

        Assert.NotNull(result);
        Assert.Single(result!.UpcomingEvents);
        Assert.Equal("Published event", result.UpcomingEvents.Single().Title);
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
