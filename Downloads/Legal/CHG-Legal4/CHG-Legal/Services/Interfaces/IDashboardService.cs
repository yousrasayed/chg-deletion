// CHG_Legal/Services/Interfaces/IDashboardService.cs
using CHG_Legal.Models.ViewModels;
using System.Threading.Tasks;

namespace CHG_Legal.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}