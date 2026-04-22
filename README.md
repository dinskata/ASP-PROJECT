# Eventure

DEPLOY URL: 

Eventure is an ASP.NET Core MVC web application for discovering events, buying tickets, managing venues, moderating reviews, and handling role-based administration. The project was built as an individual course assignment for ASP.NET Advanced and was expanded beyond a simple event catalog into a small multi-role event platform.

## What The Project Does

Visitors can:

- browse upcoming and ended events
- filter events by search, category, and status
- explore venue pages with ratings and hosted-event history
- read announcements and contact the team

Registered buyers can:

- create an account and sign in with ASP.NET Identity
- buy tickets through a test payment flow
- see purchases in a personal dashboard
- open ticket details, verification codes, and demo QR codes
- request refunds when the event is still more than 48 hours away
- review attended events after they have ended

Staff roles have their own working areas:

- `Venue Manager` can manage assigned venues, create events for those venues, and manage venue staff access
- `Venue Staff` can verify tickets for assigned venues
- `Site Moderator` can moderate reviews, view ticket activity, and manage venue-manager assignments
- `Administrator` can manage users, roles, venues, events, reviews, payments, refunds, ticket activity, and audit history

## Main Features

- public event catalog with filtering and chronological status grouping
- venue directory with ratings based on hosted-event reviews
- announcements page with pinned and recent updates
- ticket purchase flow with accepted / denied test payment actions
- per-ticket details with unique ticket code, verification code, seat, and QR demo
- ticket verification flow for venue operations
- ticket check-in tracking so the same ticket cannot be reused
- refund requests with business-rule checks
- review flow limited to verified attendees after event completion
- admin, moderator, and manager areas with scoped permissions
- audit logging for important platform actions
- seeded demo data for roles, venues, events, purchases, reviews, and tickets

## Tech Stack

- ASP.NET Core MVC
- .NET 9
- Razor Views and partial views
- Entity Framework Core
- Microsoft SQL Server / LocalDB
- ASP.NET Core Identity
- Bootstrap 5 and custom CSS
- xUnit for unit testing

## Project Structure

- `Controllers` contains the public MVC controllers
- `Areas/Admin` contains administrator-only pages and workflows
- `Areas/Manager` contains venue manager pages and actions
- `Areas/Moderator` contains moderation tools
- `Areas/Identity` contains the customized Identity account pages
- `Services` contains the business logic layer
- `Data` contains the EF Core context, migrations, bootstrap logic, and seeding
- `Models` contains both entity models and view models
- `Views` contains Razor views, shared layouts, and partials
- `ASP PROJECT.Tests` contains unit tests for service logic

## Entity Models

The project uses more than the minimum required number of entity models. The main persisted models are:

- `ApplicationUser`
- `Category`
- `Venue`
- `Event`
- `Registration`
- `RegistrationTicket`
- `Review`
- `Announcement`
- `AuditLog`
- `UserVenueAssignment`

## Architecture Notes

The application follows a layered MVC structure. Controllers stay relatively thin and delegate business rules to services. Data access is handled through EF Core and `ApplicationDbContext`. Identity handles authentication and role membership, while MVC Areas separate public pages from privileged back-office workflows.

That structure made it easier to add extra roles and features later in the project, especially ticket verification, review moderation, scoped venue management, and audit history.

## Validation, Security, And Error Handling

The project uses the built-in ASP.NET Core and Razor protections as the main security baseline:

- ASP.NET Identity for authentication and role authorization
- antiforgery protection on form submissions
- server-side validation with data annotations and model state checks
- client-side validation in the main forms
- EF Core parameterized queries to reduce SQL injection risk
- Razor HTML encoding for user-provided content
- role-based authorization on admin, moderator, manager, and verification flows

Custom status pages are included for:

- `400 Bad Request`
- `404 Not Found`
- `500 Server Error`

## Search, Filtering, And Navigation

The assignment asks for search or filtering, and the project includes both:

- event search, category filtering, and status filtering
- venue search
- filtering and sorting in admin management screens
- ticket lookup and verification filters for operational roles

Pagination is currently used on the venue directory and on several management listings. The public event catalog is intentionally shown as a longer chronological list because that suited the final browsing flow better.

## Seeded Demo Data

On startup, the app applies migrations and seeds:

- categories
- venues
- upcoming and ended events
- announcements
- demo registrations and tickets
- approved reviews
- role accounts
- venue assignments

Seeded login accounts:

- `admin@eventure.local` / `Admin123!` => `Administrator`, `Buyer`
- `test@test.com` / `Test1234` => `Buyer`
- `venuemanager@eventure.local` / `Venue1234` => `Venue Manager`, `Buyer`
- `venuestaff@eventure.local` / `Staff1234` => `Venue Staff`
- `moderator@eventure.local` / `Moder1234` => `Site Moderator`, `Buyer`
- `buyer@eventure.local` / `Buyer123!` => `Buyer`
- `reviewer@eventure.local` / `Reviewer123!` => `Buyer`

## Running The Project Locally

1. Open the project in Visual Studio 2022 or JetBrains Rider.
2. Make sure SQL Server LocalDB or another SQL Server instance is available.
3. Check the `DefaultConnection` value in `appsettings.json` if you want to use a different database.
4. Restore NuGet packages.
5. Run the project:

```powershell
dotnet run --project ".\\ASP PROJECT.csproj"
```

The application applies migrations on startup and then seeds the database with sample data.

## Tests

Unit tests are included in `ASP PROJECT.Tests`. The current test project focuses on service-layer behavior, including:

- event filtering
- purchase and seat-availability rules
- refund / repurchase scenarios
- review eligibility rules
- venue search and paging behavior
- announcement listing logic
- dashboard summary behavior

Run the tests with:

```powershell
dotnet test ".\\ASP PROJECT.Tests\\ASP PROJECT.Tests.csproj"
```

## Deployment

The project is prepared for deployment with SQL Server. The safest match for the course criteria is:

- Azure App Service for hosting
- Azure SQL Database for production data

Before final submission, the deployment section should include:

- the public live URL
- the production database choice
- any deployment-specific notes or screenshots

## Course Criteria Notes

From the codebase side, the project covers the main technical expectations:

- ASP.NET Core and .NET 6+ requirement
- more than 15 views
- more than 6 entity models
- more than 5 controllers
- Razor views, partials, and areas
- EF Core with SQL Server
- Identity and role-based authorization
- seeded relevant data
- search and filtering
- custom error pages
- dependency injection

The remaining submission quality depends on the final non-code items too, especially:

- public deployment
- strong test coverage proof if required by the evaluator
- a complete Git history that satisfies the course rules

## Final Notes

This project started as an event management idea, but the final version is closer to a small platform with public browsing, ticket handling, operations support, moderation, and administration. The goal was not just to check off the technical requirements, but to make the roles and workflows feel believable enough for a real course final project.
