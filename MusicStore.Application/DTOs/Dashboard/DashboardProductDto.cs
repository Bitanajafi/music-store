using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Dashboard
{
    public class DashboardProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string SKU { get; set; } = null!;

        public int StockQuantity { get; set; }
    }
}
