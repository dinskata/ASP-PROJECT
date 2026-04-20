using ASP_PROJECT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Data;

public static class DbInitializer
{
    public const string AdministratorRole = "Administrator";
    public const string UserRole = "User";
    public const string AdminEmail = "admin@eventure.local";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        foreach (var roleName in new[] { AdministratorRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { Name = "Technology", Description = "Workshops and talks for developers and digital teams." },
                new Category { Name = "Design", Description = "Creative sessions for product, branding, and UX." },
                new Category { Name = "Business", Description = "Events for founders, managers, and entrepreneurs." },
                new Category { Name = "Community", Description = "Local networking, volunteering, and public initiatives." });
        }

        if (!await dbContext.Venues.AnyAsync())
        {
            dbContext.Venues.AddRange(
                new Venue { Name = "Skyline Hall", City = "Sofia", Address = "18 Central Boulevard", Capacity = 220, Description = "Modern hall for mid-sized conferences and demo days." },
                new Venue { Name = "Riverside Studio", City = "Plovdiv", Address = "42 River Street", Capacity = 90, Description = "Flexible workshop studio with lounge areas and projector setup." },
                new Venue { Name = "Innovation Loft", City = "Varna", Address = "7 Harbor Avenue", Capacity = 150, Description = "Coastal venue designed for startup meetups and panel sessions." });
        }

        await dbContext.SaveChangesAsync();

        if (!await dbContext.Events.AnyAsync())
        {
            var technology = await dbContext.Categories.SingleAsync(x => x.Name == "Technology");
            var design = await dbContext.Categories.SingleAsync(x => x.Name == "Design");
            var business = await dbContext.Categories.SingleAsync(x => x.Name == "Business");
            var community = await dbContext.Categories.SingleAsync(x => x.Name == "Community");

            var skyline = await dbContext.Venues.SingleAsync(x => x.Name == "Skyline Hall");
            var studio = await dbContext.Venues.SingleAsync(x => x.Name == "Riverside Studio");
            var loft = await dbContext.Venues.SingleAsync(x => x.Name == "Innovation Loft");

            dbContext.Events.AddRange(
                new Event
                {
                    Title = "Modern ASP.NET Core Summit",
                    Description = "A one-day event focused on advanced ASP.NET Core architecture, security, clean services, and deployment workflows for production-ready applications.",
                    CategoryId = technology.Id,
                    VenueId = skyline.Id,
                    StartsAtUtc = DateTime.UtcNow.AddDays(10).Date.AddHours(8),
                    DurationMinutes = 420,
                    Price = 39,
                    SeatsAvailable = 180,
                    IsPublished = true
                },
                new Event
                {
                    Title = "UX Research Sprint Lab",
                    Description = "Hands-on design workshop covering research planning, user interviews, usability reviews, and turning feedback into sharper digital products.",
                    CategoryId = design.Id,
                    VenueId = studio.Id,
                    StartsAtUtc = DateTime.UtcNow.AddDays(14).Date.AddHours(10),
                    DurationMinutes = 240,
                    Price = 25,
                    SeatsAvailable = 70,
                    IsPublished = true
                },
                new Event
                {
                    Title = "Startup Finance Basics",
                    Description = "Practical session on pricing, runway planning, simple reporting, and investor-ready thinking for early-stage companies and side projects.",
                    CategoryId = business.Id,
                    VenueId = loft.Id,
                    StartsAtUtc = DateTime.UtcNow.AddDays(18).Date.AddHours(9),
                    DurationMinutes = 180,
                    Price = 19,
                    SeatsAvailable = 85,
                    IsPublished = true
                },
                new Event
                {
                    Title = "Neighborhood Volunteer Meetup",
                    Description = "Community meetup connecting local organizers with volunteers who want to support educational, environmental, and social causes.",
                    CategoryId = community.Id,
                    VenueId = skyline.Id,
                    StartsAtUtc = DateTime.UtcNow.AddDays(22).Date.AddHours(17),
                    DurationMinutes = 120,
                    Price = 0,
                    SeatsAvailable = 120,
                    IsPublished = true
                });
        }

        if (!await dbContext.Announcements.AnyAsync())
        {
            dbContext.Announcements.AddRange(
                new Announcement
                {
                    Title = "Registration is open for the spring event season",
                    Content = "Browse the latest events, compare venues, and reserve your seat early because the most popular workshops are filling up quickly.",
                    Audience = "All",
                    IsPinned = true
                },
                new Announcement
                {
                    Title = "Student discounts available for selected workshops",
                    Content = "Use your academic email when registering and check the event details page to see where discounted tickets are enabled.",
                    Audience = "Students",
                    IsPinned = false
                });
        }

        await dbContext.SaveChangesAsync();

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Platform Administrator"
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded administrator user.");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdministratorRole))
        {
            await userManager.AddToRoleAsync(adminUser, AdministratorRole);
        }

        if (!await userManager.IsInRoleAsync(adminUser, UserRole))
        {
            await userManager.AddToRoleAsync(adminUser, UserRole);
        }
    }
}
