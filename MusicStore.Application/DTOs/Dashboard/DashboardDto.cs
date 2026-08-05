using System;
using System.Collections.Generic;
using System.Text;
namespace MusicStore.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int TotalProducts { get; set; }

        public int ActiveProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalBrands { get; set; }

        public int TotalUsers { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
