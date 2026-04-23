# Eventure

Live demo: https://asp-project-1x6j.onrender.com/

Eventure is an ASP.NET Core MVC web application for browsing events, purchasing tickets, managing venues, moderating reviews, and handling role-based administration. The project was developed as an individual ASP.NET Advanced course assignment and grew from a basic event listing idea into a multi-role platform with operational workflows, ticket verification, refund handling, and audit tracking.

## Project Concept

The main idea behind Eventure is to present a realistic event platform rather than a simple CRUD catalog. Public visitors can explore events, venues, and announcements, while authenticated users can buy tickets, manage their purchases, and leave reviews after attending finished events.

The platform also includes internal workflows for venue operations and site administration. Venue managers can manage the venues assigned to them and create events only for those venues. Venue staff can verify tickets on entry. Site moderators can review user-generated content and supervise operational data. Administrators have full access to users, roles, venues, events, payments, tickets, and audit history.

## User Roles

The application supports several distinct roles with different responsibilities:

- `Buyer`  
  The standard registered user role. Buyers can purchase tickets, view active purchases, request refunds when allowed, and submit reviews for events they actually attended.

- `Venue Manager`  
  Manages only assigned venues. This role can create and edit events for those venues and manage venue staff access for the same scope.

- `Venue Staff`  
  Focused on ticket verification. This role can inspect tickets and mark them as checked in for the assigned venue.

- `Site Moderator`  
  Moderates reviews, supervises ticket-related activity, and helps maintain platform content and assignments.

- `Administrator`  
  Has full platform access, including users and roles, venues, events, reviews, payments and refunds, ticket history, and audit records.

## Functional Overview

### Public Area

The public part of the site allows visitors to:

- browse upcoming and ended events
- filter events by search term, category, and event status
- view event details with pricing, timing, venue information, and ticket purchase actions
- browse venue pages and see hosted-event history
- read platform announcements
- contact the Eventure team

### Buyer Experience

After signing in, a buyer can:

- purchase tickets through a test payment flow
- see purchases on the personal dashboard
- open generated tickets with unique ticket codes, verification codes, and demo QR codes
- request a refund when the event is more than 48 hours away
- review ended events only when the buyer has a valid attended purchase

### Operations And Back Office

The administrative side of the application supports:

- venue-scoped event management
- venue-staff assignment
- ticket verification and check-in
- prevention of duplicate ticket reuse
- review moderation
- payment and refund overview
- per-user purchase and ticket inspection
- audit history for key platform actions

## Main Features

- multi-role ASP.NET Identity setup
- event catalog with filtering and status grouping
- venue directory with calculated venue ratings
- announcements with pinned and recent sections
- test payment flow for ticket purchases
- ticket wallet with seat details, verification codes, and printable ticket views
- refund handling with business-rule restrictions
- review flow limited to verified attendees after event completion
- ticket verification screen for operational roles
- checked-in ticket history and one-time entry tracking
- admin, moderator, and manager areas
- seeded sample data across the whole platform

## Application Architecture

Eventure follows a layered ASP.NET Core MVC structure.

### Presentation Layer

The presentation layer is built with:

- ASP.NET Core MVC controllers
- Razor views
- shared layouts and partial views
- Bootstrap-based responsive structure with custom styling

The application uses MVC Areas to separate the public experience from privileged role panels:

- `Areas/Admin`
- `Areas/Manager`
- `Areas/Moderator`
- `Areas/Identity`

### Service Layer

Business logic is concentrated in services, which keeps the controllers thinner and easier to reason about. The main service interfaces and implementations are located in the `Services` folder.

Current service responsibilities include:

- event listing, purchase rules, reviews, refunds, and ticket generation
- venue listing and venue detail aggregation
- announcements
- dashboard summaries
- management features for roles, tickets, payments, moderation, and audit history

### Data Layer

Data access is handled with Entity Framework Core through `ApplicationDbContext`. The project uses SQL Server and applies EF Core migrations on startup. Database seeding is also performed on startup so a fresh environment gets enough content to demonstrate the full workflow.

## Entity Model

The project includes more than the minimum required number of entity models. The most important persisted models are:

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

These models support both the public event functionality and the internal management workflows.

## Important Workflow Decisions

Several project decisions were made to keep the platform closer to a believable real-world system:

- tickets are purchased, not just “registered”
- reviews are allowed only after an event has ended
- a user must have a valid attended purchase to review an event
- refunds are restricted when the event is too close
- checked-in tickets are marked as used so they cannot be reused for entry
- venue managers do not have global event control; they are limited to assigned venues
- venue staff only work within assigned venue scope

## Validation, Security, And Error Handling

The project uses the standard ASP.NET Core and Razor safety mechanisms as the main security baseline.

### Validation

Validation is applied in both the UI layer and the server layer through:

- data annotations
- model state checks
- typed input models
- client-side validation for the main forms

### Security

The application includes:

- ASP.NET Core Identity for authentication
- role-based authorization for privileged pages and actions
- antiforgery validation for POST requests
- Razor HTML encoding for displayed content
- EF Core parameterized queries to reduce SQL injection risk

### Error Handling

Custom error pages are included for:

- `400 Bad Request`
- `404 Not Found`
- `500 Server Error`

Status code handling is configured through the standard ASP.NET Core middleware pipeline.

## Search, Filtering, And Navigation

Eventure includes both search and filtering throughout the application.

- public event search by title and category
- event status filtering for upcoming and ended items
- venue search by name and city
- sorting and filtering in management pages
- ticket verification search by ticket data

Pagination is used where it still makes sense, especially on venue and management listings. The public event catalog is intentionally shown as a larger chronological feed because it reads better for the final browsing experience.

## Seeding Strategy

On startup, the application:

1. applies pending EF Core migrations
2. creates any missing roles
3. seeds categories, venues, events, announcements, purchases, reviews, tickets, and assignments
4. ensures the demo accounts exist with the expected roles

This makes the project easier to review because a fresh database still contains enough meaningful data to demonstrate both the public and privileged workflows.

## Seeded Accounts

The following demo accounts are seeded for testing:

- `admin@eventure.local` / `Admin123!`  
  Roles: `Administrator`, `Buyer`

- `test@test.com` / `Test1234`  
  Roles: `Buyer`

- `venuemanager@eventure.local` / `Venue1234`  
  Roles: `Venue Manager`, `Buyer`

- `venuestaff@eventure.local` / `Staff1234`  
  Roles: `Venue Staff`

- `moderator@eventure.local` / `Moder1234`  
  Roles: `Site Moderator`, `Buyer`

- `buyer@eventure.local` / `Buyer123!`  
  Roles: `Buyer`

- `reviewer@eventure.local` / `Reviewer123!`  
  Roles: `Buyer`

## Local Setup

### Requirements

- Visual Studio 2022 or JetBrains Rider
- .NET 9 SDK
- SQL Server LocalDB or another SQL Server instance

### Steps

1. Clone the repository.
2. Open the project in Visual Studio 2022 or JetBrains Rider.
3. Check `appsettings.json` and update the SQL Server connection string if needed.
4. Restore NuGet packages.
5. Start the application:

```powershell
dotnet run --project ".\ASP PROJECT.csproj"
```

On startup, the application migrates the database and runs the seeding process automatically.

## Testing

Unit tests are included in the `ASP PROJECT.Tests` project. The current test suite covers the main service layer scenarios that were most important for the business logic:

- event filtering
- purchase rules and seat availability
- repurchase after refund
- review eligibility after event completion
- venue listing behavior
- announcement listing behavior
- dashboard logic

Run the tests with:

```powershell
dotnet test ".\ASP PROJECT.Tests\ASP PROJECT.Tests.csproj" -c Release
```

## Deployment

The project is designed to work with SQL Server in both local and hosted environments.

For the public demo, the application can be deployed to a public hosting provider while still using a Microsoft SQL Server database. The main things needed for deployment are:

- a valid production SQL Server connection string
- `ASPNETCORE_ENVIRONMENT=Production`
- a published or containerized ASP.NET Core build

## Technologies Used

- ASP.NET Core MVC
- .NET 9
- Razor
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- Bootstrap 5
- xUnit

## Design Notes

The project was intentionally pushed beyond simple generated CRUD. The focus was to make the flows feel more realistic:

- buyers purchase tickets instead of only registering
- tickets have identity data and verification flow
- event reviews are tied to attendance and timing
- operational roles have limited scope instead of global access
- administrators can inspect a traceable audit history

That made the project more complex, but also more suitable for an advanced ASP.NET assignment with layered architecture and multiple user journeys.
