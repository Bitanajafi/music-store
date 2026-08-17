using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;

namespace MusicStore.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymentGateway paymentGateway)
        {
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
        }





        public async Task<ServiceResult<PaymentDto>> CreatePaymentAsync(
            int orderId,
            string userId)
        {
            var order = await _unitOfWork
                .Repository<Order>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == orderId &&
                         x.UserId == userId);

            if (order == null)
            {
                return ServiceResult<PaymentDto>
                    .Fail("سفارش موردنظر یافت نشد.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return ServiceResult<PaymentDto>
                    .Fail("این سفارش قابل پرداخت نیست.");
            }


            var payment = await _unitOfWork
                .Repository<Payment>()
                .GetFirstOrDefaultAsync(
                    x => x.OrderId == orderId);

            if (payment == null)
            {
                return ServiceResult<PaymentDto>
                    .Fail("اطلاعات پرداخت سفارش یافت نشد.");
            }


            if (payment.Status == PaymentStatus.Paid)
            {
                return ServiceResult<PaymentDto>
                    .Fail("این سفارش قبلاً پرداخت شده است.");
            }


            if (payment.Method != PaymentMethod.Online)
            {
                return ServiceResult<PaymentDto>
                    .Fail("این سفارش برای پرداخت آنلاین ثبت نشده است.");
            }


            // اگر قبلاً یک پرداخت ناموفق داشته،
            // برای تلاش جدید دوباره Pending می‌شه.
            payment.Status = PaymentStatus.Pending;


            var gatewayResult = await _paymentGateway
                .CreatePaymentAsync(
                    order.Id,
                    payment.Amount);


            if (!gatewayResult.Success)
            {
                payment.Status = PaymentStatus.Failed;

                await _unitOfWork
                    .Repository<Payment>()
                    .UpdateAsync(payment);

                await _unitOfWork.SaveAsync();

                return ServiceResult<PaymentDto>
                    .Fail(gatewayResult.Message);
            }


            payment.TransactionId = gatewayResult.Data;
            payment.Status = PaymentStatus.Pending;

            await _unitOfWork
                .Repository<Payment>()
                .UpdateAsync(payment);

            await _unitOfWork.SaveAsync();


            return ServiceResult<PaymentDto>.Ok(
                new PaymentDto
                {
                    Amount = payment.Amount,
                    Status = payment.Status,
                    Method = payment.Method,
                    OrderId = payment.OrderId,
                    TransactionId = payment.TransactionId,
                    PaidAt = payment.PaidAt
                },
                "پرداخت آنلاین ایجاد شد.");
        }



        public async Task<ServiceResult<PaymentDto>> VerifyPaymentAsync(
            string transactionId,
            string userId,
            bool success)
        {
            var payment = await _unitOfWork
                .Repository<Payment>()
                .GetFirstOrDefaultAsync(
                    x => x.TransactionId == transactionId);


            if (payment == null)
            {
                return ServiceResult<PaymentDto>
                    .Fail("تراکنش موردنظر یافت نشد.");
            }


            var order = await _unitOfWork
                .Repository<Order>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == payment.OrderId &&
                         x.UserId == userId,
                    query => query
                        .Include(x => x.OrderHistory));


            if (order == null)
            {
                return ServiceResult<PaymentDto>
                    .Fail("سفارش موردنظر یافت نشد.");
            }


            if (payment.Status == PaymentStatus.Paid)
            {
                return ServiceResult<PaymentDto>
                    .Fail("این پرداخت قبلاً تأیید شده است.");
            }


            var gatewayResult = await _paymentGateway
                .VerifyPaymentAsync(
                    transactionId,
                    payment.Amount,
                    success);



            if (!gatewayResult.Success)
            {
                payment.Status = PaymentStatus.Failed;


                order.OrderHistory.Add(
                    new OrderHistory
                    {
                        OldStatus = order.Status,
                        NewStatus = order.Status,
                        Description = "تلاش برای پرداخت ناموفق بود.",
                        ChangedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    });


                await _unitOfWork
                    .Repository<Payment>()
                    .UpdateAsync(payment);

                await _unitOfWork
                    .Repository<Order>()
                    .UpdateAsync(order);

                await _unitOfWork.SaveAsync();



                return ServiceResult<PaymentDto>.Ok(
                    new PaymentDto
                    {
                        Amount = payment.Amount,
                        Status = payment.Status,
                        Method = payment.Method,
                        OrderId = payment.OrderId,
                        TransactionId = payment.TransactionId,
                        PaidAt = payment.PaidAt
                    },
                    "پرداخت ناموفق بود.");
            }


            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;


            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;


            order.OrderHistory.Add(
                new OrderHistory
                {
                    OldStatus = OrderStatus.Pending,
                    NewStatus = OrderStatus.Paid,
                    Description = "پرداخت با موفقیت انجام شد.",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });


            await _unitOfWork
                .Repository<Payment>()
                .UpdateAsync(payment);

            await _unitOfWork
                .Repository<Order>()
                .UpdateAsync(order);

            await _unitOfWork.SaveAsync();


            return ServiceResult<PaymentDto>.Ok(
                new PaymentDto
                {
                    Amount = payment.Amount,
                    Status = payment.Status,
                    Method = payment.Method,
                    OrderId = payment.OrderId,
                    TransactionId = payment.TransactionId,
                    PaidAt = payment.PaidAt
                },
                "پرداخت با موفقیت تأیید شد.");
        }
    }
}