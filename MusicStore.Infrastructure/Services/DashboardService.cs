using MusicStore.Application.DTOs.Dashboard;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class DashboardService:IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public async Task<DashboardDto> GetDashboardAsync()
        {
            var totalProducts = await _unitOfWork
                .Repository<Product>()
                .CountAsync();

            var activeProducts = await _unitOfWork
                .Repository<Product>()
                .CountAsync(x => x.IsActive);

            var totalCategories = await _unitOfWork
                .Repository<Category>()
                .CountAsync();

            var totalBrands = await _unitOfWork
                .Repository<Brand>()
                .CountAsync();

            var totalUsers = await _unitOfWork
                .Repository<ApplicationUser>()
                .CountAsync();

            var totalOrders = await _unitOfWork
                .Repository<Order>()
                .CountAsync();


            return new DashboardDto
            {
                TotalProducts = totalProducts,

                ActiveProducts = activeProducts,

                TotalCategories = totalCategories,

                TotalBrands = totalBrands,

                TotalUsers = totalUsers,

                LastUpdated = DateTime.Now

            };
        }
    }
    
}
