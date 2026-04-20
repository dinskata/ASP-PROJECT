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

        var categoriesToSeed = new[]
        {
            new Category { Name = "Technology", Description = "Workshops and talks for developers and digital teams." },
            new Category { Name = "Design", Description = "Creative sessions for product, branding, and UX." },
            new Category { Name = "Business", Description = "Events for founders, managers, and entrepreneurs." },
            new Category { Name = "Community", Description = "Local networking, volunteering, and public initiatives." },
            new Category { Name = "Marketing", Description = "Sessions focused on campaigns, brand communication, and audience growth." },
            new Category { Name = "Career", Description = "Learning opportunities for job seekers, mentors, and professional development." }
        };

        foreach (var category in categoriesToSeed)
        {
            if (!await dbContext.Categories.AnyAsync(x => x.Name == category.Name))
            {
                dbContext.Categories.Add(category);
            }
        }

        var venuesToSeed = new[]
        {
            new Venue { Name = "Skyline Hall", City = "Sofia", Address = "18 Central Boulevard", Capacity = 220, Description = "Modern hall for mid-sized conferences and demo days." },
            new Venue { Name = "Riverside Studio", City = "Plovdiv", Address = "42 River Street", Capacity = 90, Description = "Flexible workshop studio with lounge areas and projector setup." },
            new Venue { Name = "Innovation Loft", City = "Varna", Address = "7 Harbor Avenue", Capacity = 150, Description = "Coastal venue designed for startup meetups and panel sessions." },
            new Venue { Name = "Central Forum Center", City = "Burgas", Address = "11 Seaside Square", Capacity = 260, Description = "Large conference center suited for keynote sessions, panels, and sponsor spaces." },
            new Venue { Name = "North Hub Workspace", City = "Ruse", Address = "25 Danube Park", Capacity = 110, Description = "Collaborative venue for practical training sessions, mentoring circles, and networking evenings." }
        };

        foreach (var venue in venuesToSeed)
        {
            if (!await dbContext.Venues.AnyAsync(x => x.Name == venue.Name))
            {
                dbContext.Venues.Add(venue);
            }
        }

        await dbContext.SaveChangesAsync();

        var technology = await dbContext.Categories.SingleAsync(x => x.Name == "Technology");
        var design = await dbContext.Categories.SingleAsync(x => x.Name == "Design");
        var business = await dbContext.Categories.SingleAsync(x => x.Name == "Business");
        var community = await dbContext.Categories.SingleAsync(x => x.Name == "Community");
        var marketing = await dbContext.Categories.SingleAsync(x => x.Name == "Marketing");
        var career = await dbContext.Categories.SingleAsync(x => x.Name == "Career");

        var skyline = await dbContext.Venues.SingleAsync(x => x.Name == "Skyline Hall");
        var studio = await dbContext.Venues.SingleAsync(x => x.Name == "Riverside Studio");
        var loft = await dbContext.Venues.SingleAsync(x => x.Name == "Innovation Loft");
        var forum = await dbContext.Venues.SingleAsync(x => x.Name == "Central Forum Center");
        var northHub = await dbContext.Venues.SingleAsync(x => x.Name == "North Hub Workspace");

        var eventsToSeed = new[]
        {
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
            },
            new Event
            {
                Title = "Growth Marketing Planning Day",
                Description = "A practical event for small teams covering campaign planning, landing page messaging, channel testing, and measuring what actually improves conversions.",
                CategoryId = marketing.Id,
                VenueId = forum.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(26).Date.AddHours(9),
                DurationMinutes = 300,
                Price = 29,
                SeatsAvailable = 140,
                IsPublished = true
            },
            new Event
            {
                Title = "Career Switch Workshop",
                Description = "An interactive workshop for people preparing for a new role, with sessions on portfolio thinking, interview preparation, and building a realistic growth plan.",
                CategoryId = career.Id,
                VenueId = northHub.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(30).Date.AddHours(11),
                DurationMinutes = 210,
                Price = 15,
                SeatsAvailable = 95,
                IsPublished = true
            },
            new Event
            {
                Title = "Product Design Critique Evening",
                Description = "A collaborative evening where designers and product teams review interfaces, discuss usability decisions, and exchange practical feedback on live case studies.",
                CategoryId = design.Id,
                VenueId = loft.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(34).Date.AddHours(15),
                DurationMinutes = 150,
                Price = 12,
                SeatsAvailable = 80,
                IsPublished = true
            },
            new Event
            {
                Title = "Community Fundraising Forum",
                Description = "A local forum exploring volunteer coordination, community outreach, donation communication, and responsible planning for public-impact initiatives.",
                CategoryId = community.Id,
                VenueId = forum.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(40).Date.AddHours(10),
                DurationMinutes = 240,
                Price = 0,
                SeatsAvailable = 170,
                IsPublished = true
            }
        };

        foreach (var eventItem in eventsToSeed)
        {
            if (!await dbContext.Events.AnyAsync(x => x.Title == eventItem.Title))
            {
                dbContext.Events.Add(eventItem);
            }
        }

        var announcementsToSeed = new[]
        {
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
            },
            new Announcement
            {
                Title = "New cities and venues have been added",
                Content = "Eventure now highlights additional venues and fresh event options so visitors can compare more locations, formats, and learning opportunities.",
                Audience = "All",
                IsPinned = false
            },
            new Announcement
            {
                Title = "Speaker and partner requests are now open",
                Content = "Teams interested in speaking, sponsoring, or supporting community sessions can use the contact page to start a collaboration conversation.",
                Audience = "Partners",
                IsPinned = true
            },
            new Announcement
            {
                Title = "Weekend community events have been refreshed",
                Content = "Several community and networking sessions were added to help visitors find more flexible weekend options across different cities.",
                Audience = "All",
                IsPinned = false
            },
            new Announcement
            {
                Title = "Venue details now include more planning information",
                Content = "Updated venue pages highlight city, address, capacity, and short descriptions to make comparing spaces easier before registration.",
                Audience = "All",
                IsPinned = false
            },
            new Announcement
            {
                Title = "Career-focused sessions added for new attendees",
                Content = "If you are preparing for a role change or building professional confidence, the new career events offer practical guidance and smaller group formats.",
                Audience = "Professionals",
                IsPinned = false
            },
            new Announcement
            {
                Title = "More workshop seats opened for selected sessions",
                Content = "Additional availability was released for some popular workshops, so visitors who missed earlier spots should check the event pages again.",
                Audience = "Attendees",
                IsPinned = false
            }
        };

        foreach (var announcement in announcementsToSeed)
        {
            if (!await dbContext.Announcements.AnyAsync(x => x.Title == announcement.Title))
            {
                dbContext.Announcements.Add(announcement);
            }
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
