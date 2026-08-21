
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Dashboard;

namespace MusicStore.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<ServiceResult<DashboardDto>> GetDashboardAsync();
    }
}

