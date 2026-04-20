using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetUserDashboardAsync(string userId, string userName);
}
