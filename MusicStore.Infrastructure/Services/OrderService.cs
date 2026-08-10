
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Application.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;
using MusicStore.Infrastructure.Repository;

namespace MusicStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly OrderStateService _orderStateService;
        private readonly ICouponService _couponService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            OrderStateService orderStateService,
            ICouponService couponService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderStateService = orderStateService;
            _couponService = couponService;
        }







        public async Task<ServiceResult<OrderDto>> CreateOrderAsync(
            CreateOrderDto dto,
            string userId)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return ServiceResult<OrderDto>
                    .Fail("سفارش باید حداقل شامل یک محصول باشد");
            }

            if (dto.ShippingInfo == null)
            {
                return ServiceResult<OrderDto>
                    .Fail("اطلاعات ارسال الزامی است");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail("تعداد محصول نامعتبر است");
                    }

                    var product = await _unitOfWork
                        .Repository<Product>()
                        .GetByIdAsync(item.ProductId);

                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail($"محصولی با شناسه {item.ProductId} یافت نشد");
                    }

                    if (!product.IsActive)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail($"محصول {product.Name} قابل سفارش نیست");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail($"موجودی محصول {product.Name} کافی نیست");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = item.Quantity
                    };

                    orderItems.Add(orderItem);

                    subTotal += orderItem.TotalPrice;

                    product.StockQuantity -= item.Quantity;

                    await _unitOfWork
                        .Repository<Product>()
                        .UpdateAsync(product);
                }

                decimal discountAmount = 0;
                Coupon? coupon = null;

                if (!string.IsNullOrWhiteSpace(dto.CouponCode))
                {
                    var couponResult = await _couponService
                        .ValidateCouponAsync(
                            dto.CouponCode,
                            subTotal);

                    if (!couponResult.Success)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(couponResult.Message);
                    }

                    discountAmount = couponResult.Data.DiscountAmount;

                    coupon = await _unitOfWork
                        .Repository<Coupon>()
                        .GetByIdAsync(couponResult.Data.CouponId);

                    if (coupon == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail("کد تخفیف یافت نشد.");
                    }

                    coupon.UsedCount++;

                    await _unitOfWork
                        .Repository<Coupon>()
                        .UpdateAsync(coupon);
                }

                var totalPrice = subTotal - discountAmount;

                var order = new Order
                {
                    UserId = userId,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    TotalPrice = totalPrice,
                    Status = OrderStatus.Pending,
                    CouponId = coupon?.Id,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in orderItems)
                {
                    order.OrderItems.Add(item);
                }

                order.ShippingInfo =
                    _mapper.Map<ShippingInfo>(dto.ShippingInfo);

                order.Payment = new Payment
                {
                    Amount = totalPrice,
                    Status = PaymentStatus.Pending,
                    Method = PaymentMethod.Online,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderHistory.Add(new OrderHistory
                {
                    OldStatus = null,
                    NewStatus = OrderStatus.Pending,
                    Description = "سفارش ایجاد شد و منتظر پرداخت است",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork
                    .Repository<Order>()
                    .AddAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>
                    .Ok(
                        result,
                        "سفارش با موفقیت ایجاد شد و منتظر پرداخت است.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail($"خطایی هنگام ثبت سفارش رخ داد: {ex.Message}");
            }
        }



        public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(
            int orderId,
            string userId)
        {
            var order = await _unitOfWork
                .Repository<Order>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == orderId && x.UserId == userId,
                    query => query
                        .Include(x => x.OrderItems)
                        .Include(x => x.ShippingInfo)
                        .Include(x => x.Payment)
                        .Include(x => x.OrderHistory));

            if (order == null)
            {
                return ServiceResult<OrderDto>
                    .Fail("سفارشی یافت نشد");
            }

            var orderDto = _mapper.Map<OrderDto>(order);

            return ServiceResult<OrderDto>.Ok(orderDto);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork
                .Repository<Order>()
                .GetAllAsync(
                    null,
                    query => query
                        .Include(x => x.OrderItems)
                        .Include(x => x.Payment)
                        .Include(x => x.ShippingInfo)
                        .Include(x => x.OrderHistory));

            var result = orders
                .OrderByDescending(x => x.CreatedAt)
                .Select(order => _mapper.Map<OrderDto>(order))
                .ToList();

            return ServiceResult<IEnumerable<OrderDto>>
                .Ok(result);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(
            string userId)
        {
            var orders = await _unitOfWork
                .Repository<Order>()
                .GetAllAsync(
                    x => x.UserId == userId,
                    query => query
                        .Include(x => x.OrderItems)
                        .Include(x => x.Payment)
                        .Include(x => x.ShippingInfo)
                        .Include(x => x.OrderHistory));

            var result = orders
                .OrderByDescending(x => x.CreatedAt)
                .Select(order => _mapper.Map<OrderDto>(order))
                .ToList();

            return ServiceResult<IEnumerable<OrderDto>>
                .Ok(result);
        }



        public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(
            int orderId,
            UpdateOrderStatusDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork
                    .Repository<Order>()
                    .GetFirstOrDefaultAsync(
                        x => x.Id == orderId,
                        query => query
                            .Include(x => x.OrderItems)
                            .Include(x => x.Payment)
                            .Include(x => x.ShippingInfo)
                            .Include(x => x.OrderHistory));

                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("سفارش موردنظر یافت نشد.");
                }

                if (order.Status == dto.Status)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("وضعیت سفارش از قبل روی همین مقدار قرار دارد.");
                }

                var oldStatus = order.Status;

                if (!_orderStateService.CanChange(
                        oldStatus,
                        dto.Status))
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("تغییر وضعیت سفارش از حالت فعلی امکان‌پذیر نیست.");
                }

                order.Status = dto.Status;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.Payment != null &&
                    dto.Status == OrderStatus.Paid)
                {
                    order.Payment.Status = PaymentStatus.Paid;
                    order.Payment.PaidAt = DateTime.UtcNow;
                }

                if (order.ShippingInfo != null &&
                    dto.Status == OrderStatus.Shipped)
                {
                    order.ShippingInfo.ShippedAt = DateTime.UtcNow;
                }

                if (order.ShippingInfo != null &&
                    dto.Status == OrderStatus.Delivered)
                {
                    order.ShippingInfo.DeliveredAt = DateTime.UtcNow;
                }

                order.OrderHistory.Add(new OrderHistory
                {
                    OldStatus = oldStatus,
                    NewStatus = dto.Status,
                    Description = dto.Description,
                    ChangedBy = "Admin",
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork
                    .Repository<Order>()
                    .UpdateAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>
                    .Ok(
                        result,
                        "وضعیت سفارش با موفقیت بروزرسانی شد.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail(
                        "هنگام بروزرسانی وضعیت سفارش خطایی رخ داد.");
            }
        }



        public async Task<ServiceResult<OrderDto>> CancelOrderAsync(
            int orderId,
            string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork
                    .Repository<Order>()
                    .GetFirstOrDefaultAsync(
                        x => x.Id == orderId && x.UserId == userId,
                        query => query
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product)
                            .Include(x => x.Payment)
                            .Include(x => x.ShippingInfo)
                            .Include(x => x.OrderHistory));

                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("سفارش موردنظر یافت نشد.");
                }

                if (order.Status != OrderStatus.Pending &&
                    order.Status != OrderStatus.Paid)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("این سفارش قابل لغو نیست.");
                }

                if (!_orderStateService.CanChange(
                        order.Status,
                        OrderStatus.Cancelled))
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail("این سفارش قابل لغو نیست.");
                }

                var oldStatus = order.Status;

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.Payment != null)
                {
                    if (order.Payment.Status == PaymentStatus.Paid)
                    {
                        order.Payment.Status = PaymentStatus.Refunded;
                    }
                    else
                    {
                        order.Payment.Status = PaymentStatus.Cancelled;
                    }
                }

                foreach (var item in order.OrderItems)
                {
                    item.Product.StockQuantity += item.Quantity;

                    await _unitOfWork
                        .Repository<Product>()
                        .UpdateAsync(item.Product);
                }

                order.OrderHistory.Add(new OrderHistory
                {
                    OldStatus = oldStatus,
                    NewStatus = OrderStatus.Cancelled,
                    Description = "سفارش توسط مشتری لغو شد.",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork
                    .Repository<Order>()
                    .UpdateAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>
                    .Ok(
                        result,
                        "سفارش با موفقیت لغو شد.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail(
                        "هنگام لغو سفارش خطایی رخ داد. لطفاً دوباره تلاش کنید.");
            }
        }






    //    //تست

    //    public async Task<ServiceResult<bool>> DeleteUserOrdersAsync(string userId)
    //    {
    //        var orders = await _unitOfWork
    //            .Repository<Order>()
    //            .GetAllAsync(x => x.UserId == userId);

    //        foreach (var order in orders)
    //        {
    //            await _unitOfWork.Repository<Order>().DeleteAsync(order);
    //        }

    //        await _unitOfWork.SaveAsync();

    //        return ServiceResult<bool>.Ok(
    //            true,
    //            "سفارش‌های تستی کاربر با موفقیت حذف شدند.");
    //    }
    //}
}














