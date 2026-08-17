using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;

namespace MusicStore.Application.Interfaces.Services;

public interface IOrderService
{
    // Customer
    Task<ServiceResult<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userId);

    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int orderId, string userId);

    Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(string userId);

    Task<ServiceResult<IEnumerable<OrderDto>>> GetAllOrdersAsync();

    Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);

    Task<ServiceResult<OrderDto>> CancelOrderAsync(int orderId, string userId);




    // Admin
    Task<ServiceResult<IEnumerable<AdminOrderListDto>>>GetAdminOrdersAsync(AdminOrderFilterDto filter);

    //Task<ServiceResult<OrderDto>> GetOrderDetailsForAdminAsync(int orderId);
}