using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Application.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;
using MusicStore.Infrastructure.Identity;
using MusicStore.Infrastructure.Repository;

namespace MusicStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly OrderStateService _orderStateService;
        private readonly ICouponService _couponService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            OrderStateService orderStateService,
            ICouponService couponService,
            UserManager<ApplicationUser> userManager,
            IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderStateService = orderStateService;
            _couponService = couponService;
            _userManager = userManager;
            _stockService = stockService;
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
                        .GetByIdAsync(
                            x => x.Id == item.ProductId,
                            x => x.Discount);



                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(
                                $"محصولی با شناسه {item.ProductId} یافت نشد");
                    }

                    if (!product.IsActive)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(
                                $"محصول {product.Name} قابل سفارش نیست");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(
                                $"موجودی محصول {product.Name} کافی نیست");
                    }


                   var finalPrice = product.Price;

                    var now = DateTime.UtcNow;

                    var discountIsActive =
                        product.Discount != null &&
                        product.Discount.IsActive &&
                        product.Discount.StartDate <= now &&
                        (product.Discount.EndDate == null ||
                         product.Discount.EndDate >= now);

                    if (discountIsActive)
                    {
                        finalPrice = product.Discount!.DiscountType ==
                            DiscountType.Percentage
                            ? product.Price -
                              (product.Price * product.Discount.Value / 100m)
                            : product.Price -
                              product.Discount.Value;

                        if (finalPrice < 0)
                            finalPrice = 0;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = finalPrice,
                        Quantity = item.Quantity
                    };





                    orderItems.Add(orderItem);

                    subTotal += orderItem.TotalPrice;

                    var stockResult = await _stockService.DecreaseStockAsync(
                    product.SKU,
                    item.Quantity,
                    "فروش محصول",
                    userId);


                    if (!stockResult.Success)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(stockResult.Message);
                    }
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
                        .GetByIdAsync(
                            couponResult.Data.CouponId);

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
                    Description =
                        "سفارش ایجاد شد و منتظر پرداخت است",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork
                    .Repository<Order>()
                    .AddAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>.Ok(
                    result,
                    "سفارش با موفقیت ایجاد شد و منتظر پرداخت است.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail(
                        $"خطایی هنگام ثبت سفارش رخ داد: {ex.Message}");
            }
        }






        public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(
            int orderId,
            string userId)
        {
            var order = await _unitOfWork
                .Repository<Order>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == orderId &&
                         x.UserId == userId,
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






        public async Task<ServiceResult<IEnumerable<OrderDto>>>
            GetAllOrdersAsync()
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





        public async Task<ServiceResult<IEnumerable<OrderDto>>>
            GetUserOrdersAsync(string userId)
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




public async Task<ServiceResult<OrderDto>>
    UpdateOrderStatusAsync(
        int orderId,
        UpdateOrderStatusDto dto,
        string userId)
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
                        .Fail(
                            "وضعیت سفارش از قبل روی همین مقدار قرار دارد.");
                }

                var oldStatus = order.Status;

                if (!_orderStateService.CanChange(
                        oldStatus,
                        dto.Status))
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>
                        .Fail(
                            "تغییر وضعیت سفارش از حالت فعلی امکان‌پذیر نیست.");
                }

                // ==========================================
                // اگر ادمین سفارش را لغو کرد
                // موجودی محصولات برگردانده می‌شود
                // ==========================================

                if (dto.Status == OrderStatus.Cancelled)
                {
                    if (oldStatus != OrderStatus.Pending &&
                        oldStatus != OrderStatus.Paid)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail("این سفارش قابل لغو نیست.");
                    }

                    if (order.Payment != null)
                    {
                        if (order.Payment.Status == PaymentStatus.Paid)
                        {
                            order.Payment.Status =
                                PaymentStatus.Refunded;
                        }
                        else
                        {
                            order.Payment.Status =
                                PaymentStatus.Cancelled;
                        }
                    }

                    foreach (var item in order.OrderItems)
                    {
                        var product = await _unitOfWork
                            .Repository<Product>()
                            .GetByIdAsync(item.ProductId);

                        if (product == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();

                            return ServiceResult<OrderDto>
                                .Fail(
                                    $"محصول مربوط به سفارش با شناسه {item.ProductId} یافت نشد.");
                        }

                        var stockResult = await _stockService
                            .IncreaseStockAsync(
                                product.SKU,
                                item.Quantity,
                                $"لغو سفارش #{order.Id} توسط ادمین",
                                userId);

                        if (!stockResult.Success)
                        {
                            await _unitOfWork.RollbackTransactionAsync();

                            return ServiceResult<OrderDto>
                                .Fail(stockResult.Message);
                        }
                    }
                }

                // ==========================================
                // منطق قبلی تغییر وضعیت سفارش
                // ==========================================

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
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork
                    .Repository<Order>()
                    .UpdateAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>.Ok(
                    result,
                    dto.Status == OrderStatus.Cancelled
                        ? "سفارش با موفقیت لغو شد و موجودی محصولات برگشت داده شد."
                        : "وضعیت سفارش با موفقیت بروزرسانی شد.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail(
                        "هنگام بروزرسانی وضعیت سفارش خطایی رخ داد.");
            }
        }






        public async Task<ServiceResult<OrderDto>>
        CancelOrderAsync(
         int orderId,
         string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork
                    .Repository<Order>()
                    .GetFirstOrDefaultAsync(
                        x => x.Id == orderId &&
                             x.UserId == userId,
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
                        order.Payment.Status =
                            PaymentStatus.Refunded;
                    }
                    else
                    {
                        order.Payment.Status =
                            PaymentStatus.Cancelled;
                    }
                }

                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork
                        .Repository<Product>()
                        .GetByIdAsync(item.ProductId);

                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(
                                $"محصول مربوط به سفارش با شناسه {item.ProductId} یافت نشد.");
                    }

                    var stockResult = await _stockService
                        .IncreaseStockAsync(
                            product.SKU,
                            item.Quantity,
                            "لغو سفارش",
                            userId);

                    if (!stockResult.Success)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return ServiceResult<OrderDto>
                            .Fail(stockResult.Message);
                    }
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

                return ServiceResult<OrderDto>.Ok(
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




        public async Task<ServiceResult<IEnumerable<AdminOrderListDto>>>
            GetAdminOrdersAsync(AdminOrderFilterDto filter)
        {
            var orders = await _unitOfWork
                .Repository<Order>()
                .GetAllAsync(
                    null,
                    query => query
                        .Include(x => x.Payment));

            var filteredOrders = orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                if (int.TryParse(search, out var orderId))
                {
                    filteredOrders = filteredOrders
                        .Where(x => x.Id == orderId);
                }
                else
                {
                    var users = await _userManager
                        .Users
                        .Where(x =>
                            (x.FirstName + " " + x.LastName)
                                .Contains(search) ||
                            (x.PhoneNumber != null &&
                             x.PhoneNumber.Contains(search)))
                        .Select(x => x.Id)
                        .ToListAsync();

                    filteredOrders = filteredOrders
                        .Where(x => users.Contains(x.UserId));
                }
            }

            if (filter.Status.HasValue)
            {
                filteredOrders = filteredOrders
                    .Where(x => x.Status == filter.Status.Value);
            }

            if (filter.PaymentStatus.HasValue)
            {
                filteredOrders = filteredOrders
                    .Where(x =>
                        x.Payment != null &&
                        x.Payment.Status ==
                        filter.PaymentStatus.Value);
            }

            if (filter.PaymentMethod.HasValue)
            {
                filteredOrders = filteredOrders
                    .Where(x =>
                        x.Payment != null &&
                        x.Payment.Method ==
                        filter.PaymentMethod.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;

                filteredOrders = filteredOrders
                    .Where(x => x.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate =
                    filter.ToDate.Value.Date.AddDays(1);

                filteredOrders = filteredOrders
                    .Where(x => x.CreatedAt < toDate);
            }

            var result = new List<AdminOrderListDto>();

            foreach (var order in filteredOrders
                .OrderByDescending(x => x.CreatedAt))
            {
                var user = await _userManager
                    .FindByIdAsync(order.UserId);

                result.Add(new AdminOrderListDto
                {
                    Id = order.Id,

                    UserId = order.UserId,

                    CustomerName = user != null
                        ? $"{user.FirstName} {user.LastName}"
                        : "کاربر حذف شده",

                    CustomerPhone = user?.PhoneNumber,

                    TotalPrice = order.TotalPrice,

                    Status = order.Status,

                    PaymentStatus = order.Payment?.Status,

                    PaymentMethod = order.Payment?.Method,

                    CreatedAt = order.CreatedAt
                });
            }

            return ServiceResult<IEnumerable<AdminOrderListDto>>
                .Ok(result);
        }




        public async Task<ServiceResult<OrderDto>>
            GetAdminOrderByIdAsync(int orderId)
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
                return ServiceResult<OrderDto>
                    .Fail("سفارش موردنظر یافت نشد.");
            }

            var result = _mapper.Map<OrderDto>(order);

            return ServiceResult<OrderDto>.Ok(result);
        }
    }
}