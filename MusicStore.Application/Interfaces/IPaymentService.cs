using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;

namespace MusicStore.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<ServiceResult<PaymentDto>> CreatePaymentAsync(
            int orderId,
            string userId);

        Task<ServiceResult<PaymentDto>> VerifyPaymentAsync(
            string transactionId,
            string userId,
            bool success);
    }
}