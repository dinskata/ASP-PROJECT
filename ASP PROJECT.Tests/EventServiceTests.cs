using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASP_PROJECT.Tests;

public class EventServiceTests
{
    [Fact]
    public async Task GetPublishedEventsAsync_FiltersBySearchTerm()
    {
        await using var dbContext = CreateDbContext(nameof(GetPublishedEventsAsync_FiltersBySearchTerm));
        dbContext.Events.AddRange(
            CreateEvent(1, "ASP.NET Deep Dive", true),
            CreateEvent(2, "Design Critique", true),
            CreateEvent(3, "Hidden Draft", false));
        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        var result = await service.GetPublishedEventsAsync("asp", null, 1, 10);

        Assert.Single(result.Result.Items);
        Assert.Equal("ASP.NET Deep Dive", result.Result.Items.Single().Title);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFalse_WhenTicketsExceedCapacity()
    {
        await using var dbContext = CreateDbContext(nameof(RegisterAsync_ReturnsFalse_WhenTicketsExceedCapacity));
        dbContext.Events.Add(CreateEvent(1, "Capacity Test", true, seatsAvailable: 2));
        await dbContext.SaveChangesAsync();
        var service = new EventService(dbContext);

        var result = await service.RegisterAsync("user-1", new RegistrationInputModel { EventId = 1, Tickets = 3 });

        Assert.False(result);
        Assert.Empty(dbContext.Registrations);
    }

    [Fact]
    public async Task RegisterAsync_CreatesRegistration_AndDecreasesSeats()
    {
        await using var dbContext = CreateDbContext(nameof(RegisterAsync_CreatesRegistration_AndDecreasesSeats));
        dbContext.Events.Add(CreateEvent(1, "Registration Test", true, seatsAvailable: 10));
        await dbContext.SaveChangesAsync();
        var service = new EventService(dbContext);

        var result = await service.RegisterAsync("user-1", new RegistrationInputModel { EventId = 1, Tickets = 2 });

        Assert.True(result);
        Assert.Single(dbContext.Registrations);
        Assert.Equal(8, dbContext.Events.Single().SeatsAvailable);
    }

    [Fact]
    public async Task AddReviewAsync_ReturnsFalse_WhenUserIsNotRegistered()
    {
        await using var dbContext = CreateDbContext(nameof(AddReviewAsync_ReturnsFalse_WhenUserIsNotRegistered));
        dbContext.Events.Add(CreateEvent(1, "Review Test", true));
        await dbContext.SaveChangesAsync();
        var service = new EventService(dbContext);

        var result = await service.AddReviewAsync("user-1", new ReviewInputModel { EventId = 1, Rating = 5, Comment = "Excellent workshop." });

        Assert.False(result);
        Assert.Empty(dbContext.Reviews);
    }

    [Fact]
    public async Task AddReviewAsync_CreatesReview_WhenUserIsRegistered()
    {
        await using var dbContext = CreateDbContext(nameof(AddReviewAsync_CreatesReview_WhenUserIsRegistered));
        dbContext.Events.Add(CreateEvent(1, "Review Test", true));
        dbContext.Registrations.Add(new Registration { EventId = 1, UserId = "user-1", Tickets = 1 });
        await dbContext.SaveChangesAsync();
        var service = new EventService(dbContext);

        var result = await service.AddReviewAsync("user-1", new ReviewInputModel { EventId = 1, Rating = 5, Comment = "Excellent workshop." });

        Assert.True(result);
        Assert.Single(dbContext.Reviews);
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var dbContext = new ApplicationDbContext(options);
        dbContext.Categories.Add(new Category { Id = 1, Name = "Technology", Description = "Desc" });
        dbContext.Venues.Add(new Venue { Id = 1, Name = "Main Hall", City = "Sofia", Address = "Address", Capacity = 100, Description = "Venue" });
        dbContext.SaveChanges();
        return dbContext;
    }

    private static Event CreateEvent(int id, string title, bool isPublished, int seatsAvailable = 50)
    {
        return new Event
        {
            Id = id,
            Title = title,
            Description = "This is a sufficiently long description for a realistic event listing.",
            CategoryId = 1,
            VenueId = 1,
            StartsAtUtc = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 120,
            Price = 20,
            SeatsAvailable = seatsAvailable,
            IsPublished = isPublished
        };
    }
}
