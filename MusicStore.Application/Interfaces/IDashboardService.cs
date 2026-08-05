using MusicStore.Application.DTOs.Dashboard;

namespace MusicStore.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}