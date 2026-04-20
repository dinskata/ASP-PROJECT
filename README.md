# Eventure

Eventure is an ASP.NET Core MVC web application for browsing, managing, and attending professional and community events. The project was prepared as an advanced individual course assignment and demonstrates a layered architecture, ASP.NET Identity, Entity Framework Core with SQL Server, MVC Areas, Razor views, validation, search, pagination, seeding, and unit testing.

## Project Concept

The platform allows visitors to:

- Browse published events with paging and filtering
- Read event details, venue details, and platform announcements
- Register for events as authenticated users
- Leave one review after attending an event
- Access a personal dashboard with their registrations

Administrators can:

- Access a dedicated `Admin` area
- Review dashboard statistics
- Create and edit events
- Publish or unpublish event listings

## Architecture

The application uses a standard layered MVC structure:

- `Controllers` handle routing, request validation, and view selection
- `Services` contain business logic and keep controllers thin
- `Data` contains the EF Core `ApplicationDbContext` and seed logic
- `Models` contain entities and view models
- `Areas/Admin` isolates administration functionality from the public site
- `Views` use Razor, partial views, layout sections, and Bootstrap-based responsive UI

## Technologies

- ASP.NET Core MVC
- .NET 9
- Razor Views
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server / LocalDB
- Bootstrap 5
- xUnit

## Domain Model

The project includes these main entity models:

- `ApplicationUser`
- `Category`
- `Venue`
- `Event`
- `Registration`
- `Review`
- `Announcement`

## Security and Validation

- ASP.NET Identity for authentication and authorization
- Role-based access with `User` and `Administrator`
- Antiforgery validation on POST actions
- Server-side and client-side validation with data annotations
- Razor HTML encoding for user-generated output
- EF Core parameterized queries to reduce SQL injection risk
- Protected admin routes through MVC Area authorization

## Features Implemented

- More than 15 views/pages
- More than 6 entity models
- More than 5 controllers
- MVC `Admin` area
- Search and category filtering for events
- Search for venues
- Pagination for event and venue listings
- Seeded categories, venues, events, announcements, roles, and administrator account
- Custom 404 and 500 error pages
- Dependency injection for services
- Responsive design

## Seeded Access

- Administrator email: `admin@eventure.local`
- Administrator password: `Admin123!`

## Setup Instructions

1. Open the solution in Visual Studio 2022 or JetBrains Rider.
2. Ensure SQL Server LocalDB or another SQL Server instance is available.
3. Update the connection string in `appsettings.json` if needed.
4. Restore NuGet packages.
5. Run the application.

On startup, the application creates the database if it does not exist and seeds initial data automatically.

## Tests

Unit tests are included in the `ASP PROJECT.Tests` project and focus on business logic in `EventService`, including:

- Filtering published events
- Registration capacity rules
- Seat count updates after registration
- Review creation rules

Run tests with:

```powershell
dotnet test ".\ASP PROJECT.Tests\ASP PROJECT.Tests.csproj"
```

## Deployment

The application is prepared for public deployment to Azure App Service or another ASP.NET-compatible host:

- Update the production SQL Server connection string
- Publish the app from Visual Studio or Rider
- Configure the production environment variables
- Deploy to Azure App Service and attach a SQL Server database

## Remaining Manual Submission Items

Two assignment items require real-world activity outside a local coding session:

- Public deployment URL
- Git history distributed across at least 7 different days with at least 30 genuine commits

Those should be completed manually during the final submission process.
