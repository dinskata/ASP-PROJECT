using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Services;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var obfuscatedConnectionString = builder.Configuration["ConnectionStrings:ObfuscatedDefaultConnection"];
if (string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(obfuscatedConnectionString))
{
    try
    {
        var bytes = Convert.FromBase64String(obfuscatedConnectionString);
        connectionString = System.Text.Encoding.UTF8.GetString(bytes);
    }
    catch (FormatException)
    {
        connectionString = null;
    }
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-ASP_PROJECT-65637b89-fe41-4c16-a765-7c3e79a8fe75;Trusted_Connection=True;MultipleActiveResultSets=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IContactRequestService, ContactRequestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IManagementService, ManagementService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (string.IsNullOrWhiteSpace(dbContext.Database.GetConnectionString()))
    {
        dbContext.Database.SetConnectionString(connectionString);
    }

    var dbConnection = dbContext.Database.GetDbConnection();
    if (string.IsNullOrWhiteSpace(dbConnection.ConnectionString))
    {
        dbConnection.ConnectionString = connectionString;
    }

    await MigrationBootstrapper.BaselineEnsureCreatedDatabaseAsync(dbContext, connectionString);
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
