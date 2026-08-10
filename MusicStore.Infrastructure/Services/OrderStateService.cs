using MusicStore.Domain.Enum;

namespace MusicStore.Application.Services
{
    public class OrderStateService
    {
        public bool CanChange(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending =>
                    newStatus == OrderStatus.Paid ||
                    newStatus == OrderStatus.Cancelled,


                OrderStatus.Paid =>
                    newStatus == OrderStatus.Processing ||
                    newStatus == OrderStatus.Cancelled,


                OrderStatus.Processing =>
                    newStatus == OrderStatus.Shipped,


                OrderStatus.Shipped =>
                    newStatus == OrderStatus.Delivered,


                OrderStatus.Delivered =>
                    false,


                OrderStatus.Cancelled =>
                    false,


                _ => false
            };
        }
    }
}
