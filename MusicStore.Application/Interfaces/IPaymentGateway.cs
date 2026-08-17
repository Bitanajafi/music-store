using MusicStore.Application.Common.Results;

namespace MusicStore.Application.Interfaces.Services
{
    public interface IPaymentGateway
    {
        Task<ServiceResult<string>> CreatePaymentAsync(
            int orderId,
            decimal amount);

        Task<ServiceResult<bool>> VerifyPaymentAsync(
            string transactionId,
            decimal amount,
            bool success);
    }
}