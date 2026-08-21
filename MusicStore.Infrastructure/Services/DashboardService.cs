
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Dashboard;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }



        private static decimal CalculateProfit(IEnumerable<Order> orders)
        {
            decimal profit = 0;

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product == null)
                    {
                        continue;
                    }

                    var revenue =
                        item.UnitPrice * item.Quantity;

                    var cost =
                        item.Product.CostPrice * item.Quantity;

                    profit += revenue - cost;
                }
            }

            return profit;
        }

        public async Task<ServiceResult<DashboardDto>>GetDashboardAsync()
        {
            try
            {
                var now = DateTime.UtcNow;

                var today = now.Date;

                var daysFromSaturday =((int)today.DayOfWeek + 1) % 7;

                var startOfWeek =today.AddDays(-daysFromSaturday);

                var startOfMonth = new DateTime(
                    today.Year,
                    today.Month,
                    1);

                var products = await _unitOfWork
                    .Repository<Product>()
                    .GetAllAsync();

                var categories = await _unitOfWork
                    .Repository<Category>()
                    .GetAllAsync();

                var brands = await _unitOfWork
                    .Repository<Brand>()
                    .GetAllAsync();

                var orders = await _unitOfWork
                    .Repository<Order>()
                    .GetAllAsync(
                        null,
                        query => query
                            .Include(x => x.Payment)
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product));

                var totalUsers = await _userManager
                    .Users
                    .CountAsync();

                var paidOrders = orders
                    .Where(x =>
                        x.Payment != null &&
                        x.Payment.Status ==
                        PaymentStatus.Paid)
                    .ToList();

                var todayPaidOrders = paidOrders
                    .Where(x =>
                        x.Payment!.PaidAt.HasValue &&
                        x.Payment.PaidAt.Value >= today)
                    .ToList();

                var weekPaidOrders = paidOrders
                    .Where(x =>
                        x.Payment!.PaidAt.HasValue &&
                        x.Payment.PaidAt.Value >= startOfWeek)
                    .ToList();

                var monthPaidOrders = paidOrders
                    .Where(x =>
                        x.Payment!.PaidAt.HasValue &&
                        x.Payment.PaidAt.Value >= startOfMonth)
                    .ToList();

                var todaySales =
                    todayPaidOrders.Sum(x => x.TotalPrice);

                var weekSales =
                    weekPaidOrders.Sum(x => x.TotalPrice);

                var monthSales =
                    monthPaidOrders.Sum(x => x.TotalPrice);

                var todayDiscount =
                    todayPaidOrders.Sum(x => x.DiscountAmount);

                var monthDiscount =
                    monthPaidOrders.Sum(x => x.DiscountAmount);

                var todayProfit =
                    CalculateProfit(todayPaidOrders);

                var monthProfit =
                    CalculateProfit(monthPaidOrders);

                var averageOrderValue =
                    paidOrders.Any()? paidOrders.Average(x => x.TotalPrice): 0;

                var inventoryValue =
                    products.Sum(
                        x => x.StockQuantity *
                             x.CostPrice);

                var outOfStockProductList = products.Where(x => x.StockQuantity <= 0)
                .Select(x => new DashboardProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    SKU = x.SKU,
                    StockQuantity = x.StockQuantity
                })
                .Take(10)
                .ToList();

                var lowStockProductList = products.Where(x =>
                        x.StockQuantity > 0 &&
                        x.StockQuantity <= 5)
                    .OrderBy(x => x.StockQuantity)
                    .Select(x => new DashboardProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        SKU = x.SKU,
                        StockQuantity = x.StockQuantity
                    })
                    .Take(10)
                    .ToList();



                var dashboard = new DashboardDto
                {
                    TotalProducts = products.Count(),

                    ActiveProducts =products.Count(x => x.IsActive),

                    TotalCategories =categories.Count(),

                    TotalBrands =brands.Count(),

                    TotalUsers =totalUsers,

                    OutOfStockProducts =products.Count(x => x.StockQuantity <= 0),

                    LowStockProducts =products.Count(x =>x.StockQuantity > 0 &&x.StockQuantity <= 5),

                    OutOfStockProductList = outOfStockProductList,

                    LowStockProductList = lowStockProductList,

                    InventoryValue =inventoryValue,

                    TotalOrders =orders.Count(),

                    PendingOrders =orders.Count(x =>x.Status ==OrderStatus.Pending),

                    PaidOrders =orders.Count(x =>x.Status ==OrderStatus.Paid),

                    ProcessingOrders =orders.Count(x =>x.Status ==OrderStatus.Processing),

                    ShippedOrders =orders.Count(x =>x.Status ==OrderStatus.Shipped),

                    DeliveredOrders =orders.Count(x => x.Status == OrderStatus.Delivered),

                    CancelledOrders =orders.Count(x =>x.Status ==OrderStatus.Cancelled),

                    TodaySales =todaySales,

                    ThisWeekSales =weekSales,

                    ThisMonthSales =monthSales,

                    TodayDiscount =todayDiscount,

                    ThisMonthDiscount = monthDiscount,

                    TodayProfit =todayProfit,

                    ThisMonthProfit =monthProfit,

                    AverageOrderValue =averageOrderValue,

                    LastUpdated =DateTime.UtcNow
                };

                return ServiceResult<DashboardDto>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardDto>
                    .Fail($"خطا در دریافت اطلاعات داشبورد: {ex.Message}");
            }
        }
    }
}

