using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Dashboard;
using MusicStore.Application.Interfaces;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }






        public async Task<IActionResult> Index()
        {
            var result = await _dashboardService.GetDashboardAsync(); 
            if (!result.Success || result.Data == null) 
            { 
                TempData["Error"] = result.Message; 
                return View(new DashboardDto()); 
            }

            return View(result.Data);
        }
    }
}
