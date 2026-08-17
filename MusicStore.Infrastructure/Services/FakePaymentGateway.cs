using MusicStore.Application.Common.Results;
using MusicStore.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class FakePaymentGateway : IPaymentGateway
    {
        public async Task<ServiceResult<string>> CreatePaymentAsync(
            int orderId,
            decimal amount)
        {
            await Task.Delay(100);

            if (amount <= 0)
            {
                return ServiceResult<string>
                    .Fail("مبلغ پرداخت نامعتبر است.");
            }

            var transactionId =
                $"TEST-{Guid.NewGuid():N}";

            return ServiceResult<string>.Ok(
                transactionId,
                "تراکنش آزمایشی با موفقیت ایجاد شد.");
        }





        public async Task<ServiceResult<bool>> VerifyPaymentAsync(
        string transactionId,
        decimal amount,
        bool success)
        {
            await Task.Delay(100);

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return ServiceResult<bool>
                    .Fail("شناسه تراکنش نامعتبر است.");
            }

            if (amount <= 0)
            {
                return ServiceResult<bool>
                    .Fail("مبلغ پرداخت نامعتبر است.");
            }

            if (!success)
            {
                return ServiceResult<bool>
                    .Fail("پرداخت توسط کاربر ناموفق بود.");
            }

            return ServiceResult<bool>.Ok(
                true,
                "پرداخت آزمایشی با موفقیت تأیید شد.");
        }
    }
}
