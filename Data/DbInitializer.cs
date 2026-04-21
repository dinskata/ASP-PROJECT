using ASP_PROJECT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Data;

public static class DbInitializer
{
    private const string DefaultLocalDbConnection = "Server=(localdb)\\mssqllocaldb;Database=aspnet-ASP_PROJECT-65637b89-fe41-4c16-a765-7c3e79a8fe75;Trusted_Connection=True;MultipleActiveResultSets=true";
    public const string AdministratorRole = "Administrator";
    public const string BuyerRole = "Buyer";
    public const string VenueManagerRole = "Venue Manager";
    public const string SiteModeratorRole = "Site Moderator";
    public const string TestUserEmail = "test@test.com";
    public const string TestUserPassword = "Test1234";
    public const string TestVenueManagerEmail = "venuemanager@eventure.local";
    public const string TestVenueManagerPassword = "Venue1234";
    public const string SiteModeratorEmail = "moderator@eventure.local";
    public const string SiteModeratorPassword = "Moder1234";
    public const string AdminEmail = "admin@eventure.local";
    public const string AdminPassword = "Admin123!";
    public const string DemoBuyerEmail = "buyer@eventure.local";
    public const string DemoBuyerPassword = "Buyer123!";
    public const string DemoReviewerEmail = "reviewer@eventure.local";
    public const string DemoReviewerPassword = "Reviewer123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (string.IsNullOrWhiteSpace(dbContext.Database.GetConnectionString()))
        {
            dbContext.Database.SetConnectionString(DefaultLocalDbConnection);
        }

        if (await dbContext.Database.CanConnectAsync() && !await RegistrationPurchaseColumnsExistAsync(dbContext))
        {
            await dbContext.Database.EnsureDeletedAsync();
        }

        await dbContext.Database.EnsureCreatedAsync();

        foreach (var roleName in new[] { AdministratorRole, BuyerRole, VenueManagerRole, SiteModeratorRole })
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
            },
            new Event
            {
                Title = "Digital Product Leadership Roundtable",
                Description = "A focused discussion event for team leads and product decision-makers covering roadmap planning, cross-functional communication, delivery tradeoffs, and healthier collaboration between product, design, and engineering.",
                CategoryId = business.Id,
                VenueId = skyline.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(44).Date.AddHours(13),
                DurationMinutes = 180,
                Price = 22,
                SeatsAvailable = 105,
                IsPublished = true
            },
            new Event
            {
                Title = "Frontend Systems Retrospective",
                Description = "A past discussion session exploring component libraries, design system decisions, and how teams can simplify front-end collaboration after shipping complex products.",
                CategoryId = technology.Id,
                VenueId = skyline.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(-20).Date.AddHours(9),
                DurationMinutes = 180,
                Price = 18,
                SeatsAvailable = 140,
                IsPublished = true
            },
            new Event
            {
                Title = "Community Organizers Workshop",
                Description = "A completed workshop for local organizers focused on volunteer coordination, communication planning, and running practical events with clearer attendee expectations.",
                CategoryId = community.Id,
                VenueId = northHub.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(-12).Date.AddHours(10),
                DurationMinutes = 150,
                Price = 0,
                SeatsAvailable = 90,
                IsPublished = true
            },
            new Event
            {
                Title = "Design Systems Review Session",
                Description = "A completed design-focused event covering component consistency, UI documentation, accessibility checks, and more practical collaboration between design and engineering teams.",
                CategoryId = design.Id,
                VenueId = studio.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(-16).Date.AddHours(11),
                DurationMinutes = 180,
                Price = 16,
                SeatsAvailable = 65,
                IsPublished = true
            },
            new Event
            {
                Title = "Local Growth Meetup Recap",
                Description = "A finished marketing meetup where attendees reviewed campaign experiments, audience growth lessons, and realistic reporting approaches for smaller product teams.",
                CategoryId = marketing.Id,
                VenueId = forum.Id,
                StartsAtUtc = DateTime.UtcNow.AddDays(-8).Date.AddHours(18),
                DurationMinutes = 120,
                Price = 10,
                SeatsAvailable = 110,
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
                PublishedOnUtc = DateTime.UtcNow.AddDays(-18),
                IsPinned = true
            },
            new Announcement
            {
                Title = "Student discounts available for selected workshops",
                Content = "Use your academic email when registering and check the event details page to see where discounted tickets are enabled.",
                Audience = "Students",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-15),
                IsPinned = false
            },
            new Announcement
            {
                Title = "New cities and venues have been added",
                Content = "Eventure now highlights additional venues and fresh event options so visitors can compare more locations, formats, and learning opportunities.",
                Audience = "All",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-12),
                IsPinned = false
            },
            new Announcement
            {
                Title = "Speaker and partner requests are now open",
                Content = "Teams interested in speaking, sponsoring, or supporting community sessions can use the contact page to start a collaboration conversation.",
                Audience = "Partners",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-10),
                IsPinned = true
            },
            new Announcement
            {
                Title = "Weekend community events have been refreshed",
                Content = "Several community and networking sessions were added to help visitors find more flexible weekend options across different cities.",
                Audience = "All",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-7),
                IsPinned = false
            },
            new Announcement
            {
                Title = "Venue details now include more planning information",
                Content = "Updated venue pages highlight city, address, capacity, and short descriptions to make comparing spaces easier before registration.",
                Audience = "All",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-5),
                IsPinned = false
            },
            new Announcement
            {
                Title = "Career-focused sessions added for new attendees",
                Content = "If you are preparing for a role change or building professional confidence, the new career events offer practical guidance and smaller group formats.",
                Audience = "Professionals",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-3),
                IsPinned = false
            },
            new Announcement
            {
                Title = "More workshop seats opened for selected sessions",
                Content = "Additional availability was released for some popular workshops, so visitors who missed earlier spots should check the event pages again.",
                Audience = "Attendees",
                PublishedOnUtc = DateTime.UtcNow.AddDays(-1),
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

        if (!await userManager.IsInRoleAsync(adminUser, BuyerRole))
        {
            await userManager.AddToRoleAsync(adminUser, BuyerRole);
        }

        var testUser = await userManager.FindByEmailAsync(TestUserEmail);
        if (testUser is null)
        {
            testUser = new ApplicationUser
            {
                UserName = TestUserEmail,
                Email = TestUserEmail,
                EmailConfirmed = true,
                FullName = "Test User"
            };

            var testUserResult = await userManager.CreateAsync(testUser, TestUserPassword);
            if (!testUserResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded test user.");
            }
        }

        if (!await userManager.IsInRoleAsync(testUser, BuyerRole))
        {
            await userManager.AddToRoleAsync(testUser, BuyerRole);
        }

        var venueManagerUser = await userManager.FindByEmailAsync(TestVenueManagerEmail);
        if (venueManagerUser is null)
        {
            venueManagerUser = new ApplicationUser
            {
                UserName = TestVenueManagerEmail,
                Email = TestVenueManagerEmail,
                EmailConfirmed = true,
                FullName = "Test Venue Manager"
            };

            var venueManagerResult = await userManager.CreateAsync(venueManagerUser, TestVenueManagerPassword);
            if (!venueManagerResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded venue manager user.");
            }
        }

        if (!await userManager.IsInRoleAsync(venueManagerUser, VenueManagerRole))
        {
            await userManager.AddToRoleAsync(venueManagerUser, VenueManagerRole);
        }

        if (!await userManager.IsInRoleAsync(venueManagerUser, BuyerRole))
        {
            await userManager.AddToRoleAsync(venueManagerUser, BuyerRole);
        }

        var siteModeratorUser = await userManager.FindByEmailAsync(SiteModeratorEmail);
        if (siteModeratorUser is null)
        {
            siteModeratorUser = new ApplicationUser
            {
                UserName = SiteModeratorEmail,
                Email = SiteModeratorEmail,
                EmailConfirmed = true,
                FullName = "Site Moderator"
            };

            var siteModeratorResult = await userManager.CreateAsync(siteModeratorUser, SiteModeratorPassword);
            if (!siteModeratorResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded site moderator user.");
            }
        }

        if (!await userManager.IsInRoleAsync(siteModeratorUser, SiteModeratorRole))
        {
            await userManager.AddToRoleAsync(siteModeratorUser, SiteModeratorRole);
        }

        if (!await userManager.IsInRoleAsync(siteModeratorUser, BuyerRole))
        {
            await userManager.AddToRoleAsync(siteModeratorUser, BuyerRole);
        }

        var demoBuyer = await userManager.FindByEmailAsync(DemoBuyerEmail);
        if (demoBuyer is null)
        {
            demoBuyer = new ApplicationUser
            {
                UserName = DemoBuyerEmail,
                Email = DemoBuyerEmail,
                EmailConfirmed = true,
                FullName = "Demo Buyer"
            };

            var buyerResult = await userManager.CreateAsync(demoBuyer, DemoBuyerPassword);
            if (!buyerResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded buyer user.");
            }
        }

        if (!await userManager.IsInRoleAsync(demoBuyer, BuyerRole))
        {
            await userManager.AddToRoleAsync(demoBuyer, BuyerRole);
        }

        var demoReviewer = await userManager.FindByEmailAsync(DemoReviewerEmail);
        if (demoReviewer is null)
        {
            demoReviewer = new ApplicationUser
            {
                UserName = DemoReviewerEmail,
                Email = DemoReviewerEmail,
                EmailConfirmed = true,
                FullName = "Demo Reviewer"
            };

            var reviewerResult = await userManager.CreateAsync(demoReviewer, DemoReviewerPassword);
            if (!reviewerResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create seeded reviewer user.");
            }
        }

        if (!await userManager.IsInRoleAsync(demoReviewer, BuyerRole))
        {
            await userManager.AddToRoleAsync(demoReviewer, BuyerRole);
        }

        var pastFrontendEvent = await dbContext.Events.SingleAsync(x => x.Title == "Frontend Systems Retrospective");
        var pastCommunityEvent = await dbContext.Events.SingleAsync(x => x.Title == "Community Organizers Workshop");
        var pastDesignEvent = await dbContext.Events.SingleAsync(x => x.Title == "Design Systems Review Session");
        var pastMarketingEvent = await dbContext.Events.SingleAsync(x => x.Title == "Local Growth Meetup Recap");

        if (!await dbContext.Registrations.AnyAsync(x => x.EventId == pastFrontendEvent.Id && x.UserId == demoBuyer.Id))
        {
            dbContext.Registrations.Add(new Registration
            {
                EventId = pastFrontendEvent.Id,
                UserId = demoBuyer.Id,
                Tickets = 1,
                RegisteredOnUtc = pastFrontendEvent.StartsAtUtc.AddDays(-10)
            });
        }

        if (!await dbContext.Registrations.AnyAsync(x => x.EventId == pastCommunityEvent.Id && x.UserId == adminUser.Id))
        {
            dbContext.Registrations.Add(new Registration
            {
                EventId = pastCommunityEvent.Id,
                UserId = adminUser.Id,
                Tickets = 2,
                RegisteredOnUtc = pastCommunityEvent.StartsAtUtc.AddDays(-8)
            });
        }

        if (!await dbContext.Registrations.AnyAsync(x => x.EventId == pastDesignEvent.Id && x.UserId == demoReviewer.Id))
        {
            dbContext.Registrations.Add(new Registration
            {
                EventId = pastDesignEvent.Id,
                UserId = demoReviewer.Id,
                Tickets = 1,
                RegisteredOnUtc = pastDesignEvent.StartsAtUtc.AddDays(-6)
            });
        }

        if (!await dbContext.Registrations.AnyAsync(x => x.EventId == pastMarketingEvent.Id && x.UserId == demoBuyer.Id))
        {
            dbContext.Registrations.Add(new Registration
            {
                EventId = pastMarketingEvent.Id,
                UserId = demoBuyer.Id,
                Tickets = 1,
                RegisteredOnUtc = pastMarketingEvent.StartsAtUtc.AddDays(-5)
            });
        }

        if (!await dbContext.Registrations.AnyAsync(x => x.EventId == pastMarketingEvent.Id && x.UserId == demoReviewer.Id))
        {
            dbContext.Registrations.Add(new Registration
            {
                EventId = pastMarketingEvent.Id,
                UserId = demoReviewer.Id,
                Tickets = 1,
                RegisteredOnUtc = pastMarketingEvent.StartsAtUtc.AddDays(-4)
            });
        }

        if (!await dbContext.Reviews.AnyAsync(x => x.EventId == pastFrontendEvent.Id && x.UserId == demoBuyer.Id))
        {
            dbContext.Reviews.Add(new Review
            {
                EventId = pastFrontendEvent.Id,
                UserId = demoBuyer.Id,
                Rating = 5,
                Comment = "Great discussions, useful examples, and a much stronger sense of how design systems can stay maintainable across teams.",
                CreatedOnUtc = pastFrontendEvent.StartsAtUtc.AddDays(1)
            });
        }

        if (!await dbContext.Reviews.AnyAsync(x => x.EventId == pastCommunityEvent.Id && x.UserId == adminUser.Id))
        {
            dbContext.Reviews.Add(new Review
            {
                EventId = pastCommunityEvent.Id,
                UserId = adminUser.Id,
                Rating = 4,
                Comment = "Well organized and practical, especially for smaller teams planning community events with limited time and resources.",
                CreatedOnUtc = pastCommunityEvent.StartsAtUtc.AddDays(1)
            });
        }

        if (!await dbContext.Reviews.AnyAsync(x => x.EventId == pastDesignEvent.Id && x.UserId == demoReviewer.Id))
        {
            dbContext.Reviews.Add(new Review
            {
                EventId = pastDesignEvent.Id,
                UserId = demoReviewer.Id,
                Rating = 5,
                Comment = "Clear examples, thoughtful feedback, and a very practical discussion about keeping design systems usable across real product work.",
                CreatedOnUtc = pastDesignEvent.StartsAtUtc.AddDays(1)
            });
        }

        if (!await dbContext.Reviews.AnyAsync(x => x.EventId == pastMarketingEvent.Id && x.UserId == demoBuyer.Id))
        {
            dbContext.Reviews.Add(new Review
            {
                EventId = pastMarketingEvent.Id,
                UserId = demoBuyer.Id,
                Rating = 4,
                Comment = "Useful session with realistic campaign advice and good examples for smaller teams that need simple reporting.",
                CreatedOnUtc = pastMarketingEvent.StartsAtUtc.AddDays(1)
            });
        }

        if (!await dbContext.Reviews.AnyAsync(x => x.EventId == pastMarketingEvent.Id && x.UserId == demoReviewer.Id))
        {
            dbContext.Reviews.Add(new Review
            {
                EventId = pastMarketingEvent.Id,
                UserId = demoReviewer.Id,
                Rating = 5,
                Comment = "Strong speaker lineup and a practical pace throughout. The conversion and messaging sections were especially helpful.",
                CreatedOnUtc = pastMarketingEvent.StartsAtUtc.AddDays(2)
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<bool> RegistrationPurchaseColumnsExistAsync(ApplicationDbContext dbContext)
    {
        await using var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'Registrations'
              AND COLUMN_NAME IN ('AmountPaid', 'CardLast4', 'CardholderName', 'PaymentStatus', 'RefundedOnUtc')
            """;

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) == 5;
    }
}
