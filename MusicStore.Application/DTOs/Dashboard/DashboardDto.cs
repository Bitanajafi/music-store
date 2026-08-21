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

       
        public int OutOfStockProducts { get; set; }

        public int LowStockProducts { get; set; }

        public decimal InventoryValue { get; set; }

        
        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int PaidOrders { get; set; }

        public int ProcessingOrders { get; set; }

        public int ShippedOrders { get; set; }

        public int DeliveredOrders { get; set; }

        public int CancelledOrders { get; set; }

       
        public decimal TodaySales { get; set; }

        public decimal ThisWeekSales { get; set; }

        public decimal ThisMonthSales { get; set; }

       
        public decimal TodayDiscount { get; set; }

        public decimal ThisMonthDiscount { get; set; }

        
        public decimal TodayProfit { get; set; }

        public decimal ThisMonthProfit { get; set; }

       
        public decimal AverageOrderValue { get; set; }


        public DateTime LastUpdated { get; set; }


        public List<DashboardProductDto> OutOfStockProductList { get; set; }= new();

        public List<DashboardProductDto> LowStockProductList { get; set; }= new();
    }
}


