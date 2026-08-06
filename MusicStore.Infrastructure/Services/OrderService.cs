using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.DTOs.OrderItem;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }






        public async Task<ServiceResult<OrderDto>> CreateOrderAsync(CreateOrderDto dto,string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
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


                var orderItems = new List<OrderItem>();

                decimal subTotal = 0;


                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        return ServiceResult<OrderDto>
                            .Fail("تعداد محصول نامعتبر است");
                    }


                    var product =
                        await _unitOfWork
                        .Repository<Product>()
                        .GetByIdAsync(item.ProductId);


                    if (product == null)
                    {
                        return ServiceResult<OrderDto>
                            .Fail($"محصولی با شناسه {item.ProductId} یافت نشد");
                    }


                    if (!product.IsActive)
                    {
                        return ServiceResult<OrderDto>
                            .Fail($"محصول {product.Name} قابل سفارش نیست");
                    }


                    if (product.StockQuantity < item.Quantity)
                    {
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
                    coupon =
                        await _unitOfWork
                        .Repository<Coupon>()
                        .GetFirstOrDefaultAsync(
                            x => x.Code == dto.CouponCode);


                    if (coupon == null)
                    {
                        return ServiceResult<OrderDto>
                            .Fail("کد تخفیف یافت نشد");
                    }


                    if (!coupon.IsActive)
                    {
                        return ServiceResult<OrderDto>
                            .Fail("کد تخفیف فعال نیست");
                    }


                    if (coupon.ExpireDate < DateTime.UtcNow)
                    {
                        return ServiceResult<OrderDto>
                            .Fail("اعتبار کد تخفیف تمام شده است");
                    }


                    if (coupon.MinimumOrderAmount.HasValue &&
                        subTotal < coupon.MinimumOrderAmount)
                    {
                        return ServiceResult<OrderDto>
                            .Fail("مبلغ سفارش برای استفاده از تخفیف کافی نیست");
                    }


                    discountAmount =
                        coupon.DiscountType == DiscountType.Percentage
                        ? subTotal * coupon.Value / 100
                        : coupon.Value;


                    if (discountAmount > subTotal)
                    {
                        discountAmount = subTotal;
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



                order.ShippingInfo = _mapper.Map<ShippingInfo>(
                    dto.ShippingInfo);



                order.Payment = new Payment
                {
                    Amount = totalPrice,
                    Status = PaymentStatus.Paid,
                    Method = PaymentMethod.Online,
                    CreatedAt = DateTime.UtcNow
                };



                order.OrderHistory.Add(new OrderHistory
                {
                    OldStatus = null,
                    NewStatus = OrderStatus.Pending,
                    Description = "سفارش با موفقیت ثبت شد",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });



                await _unitOfWork
                    .Repository<Order>()
                    .AddAsync(order);


                await _unitOfWork.SaveAsync();


                await _unitOfWork.CommitTransactionAsync();



                var result =
                    _mapper.Map<OrderDto>(order);



                return ServiceResult<OrderDto>
                    .Ok(result, "سفارش با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>
                    .Fail($"خطایی هنگام ثبت سفارش رخ داد: {ex.Message}");
            }
        }


        public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int orderId,string userId)
        {
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId,
                    query => query
                        .Include(x => x.OrderItems)
                        .Include(x => x.ShippingInfo)
                        .Include(x => x.Payment)
                        .Include(x => x.OrderHistory)
                );


            if (order == null)
            {
                return ServiceResult<OrderDto>.Fail("سفارشی یافت نشد");
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
                        .Include(x => x.OrderHistory)
                );


            var result = orders.OrderByDescending(x => x.CreatedAt)
                .Select(order => _mapper.Map<OrderDto>(order))
                .ToList();

            return ServiceResult<IEnumerable<OrderDto>>.Ok(result);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(string userId)
        {
            var orders = await _unitOfWork
                .Repository<Order>().GetAllAsync(x => x.UserId == userId,
                    query => query
                        .Include(x => x.OrderItems)
                        .Include(x => x.Payment)
                        .Include(x => x.ShippingInfo)
                        .Include(x => x.OrderHistory)
                );


            var result = orders.OrderByDescending(x => x.CreatedAt)
                .Select(order => _mapper.Map<OrderDto>(order))
                .ToList();


            return ServiceResult<IEnumerable<OrderDto>>
                .Ok(result);
        }

        public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(int orderId,UpdateOrderStatusDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(x => x.Id == orderId,
                query => query
                    .Include(x => x.OrderItems)
                    .Include(x => x.Payment)
                    .Include(x => x.ShippingInfo)
                    .Include(x => x.OrderHistory)
                    );


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



                await _unitOfWork.Repository<Order>().UpdateAsync(order);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();



                var result = _mapper.Map<OrderDto>(order);

                return ServiceResult<OrderDto>.Ok(result, "وضعیت سفارش با موفقیت بروزرسانی شد.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return ServiceResult<OrderDto>.Fail("هنگام بروزرسانی وضعیت سفارش خطایی رخ داد.");
            }
        }

        public async Task<ServiceResult<OrderDto>> CancelOrderAsync( int orderId,string userId)
        {
            await _unitOfWork.BeginTransactionAsync();


            try
            {
                var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(
                        x => x.Id == orderId && x.UserId == userId,
                        query => query
                            .Include(x => x.OrderItems)
                               .ThenInclude(x => x.Product)
                            .Include(x => x.Payment)
                            .Include(x => x.ShippingInfo)
                            .Include(x => x.OrderHistory)
                    );


                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return ServiceResult<OrderDto>.Fail("سفارش موردنظر یافت نشد.");
                }

                if (order.Status != OrderStatus.Pending &&
                    order.Status != OrderStatus.Paid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceResult<OrderDto>.Fail("این سفارش قابل لغو نیست.");
                }



                var oldStatus = order.Status;


                order.Status = OrderStatus.Cancelled;

                order.UpdatedAt = DateTime.UtcNow;




                foreach (var item in order.OrderItems)
                {
                    item.Product.StockQuantity += item.Quantity;
                    await _unitOfWork.Repository<Product>().UpdateAsync(item.Product);
                }


                order.OrderHistory.Add(new OrderHistory
                {
                    OldStatus = oldStatus,
                    NewStatus = OrderStatus.Cancelled,
                    Description = "سفارش توسط مشتری لغو شد.",
                    ChangedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });


                await _unitOfWork.Repository<Order>().UpdateAsync(order);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();



                var result = _mapper.Map<OrderDto>(order);



                return ServiceResult<OrderDto>.Ok(result, "سفارش با موفقیت لغو شد.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<OrderDto>.Fail("هنگام لغو سفارش خطایی رخ داد. لطفاً دوباره تلاش کنید.");
            }
        }








        //    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userId)
        //    {
        //       await _unitOfWork.BeginTransactionAsync();
        //        try
        //        {
        //            if (dto.Items == null || !dto.Items.Any())
        //            {
        //                return ServiceResult<OrderDto>.Fail("سفارش باید حداقل شامل یک محصول باشد");
        //            }
        //            if (dto.ShippingInfo == null)
        //            {
        //                return ServiceResult<OrderDto>.Fail("اطلاعات ارسال الزامی است");
        //            }



        //            var orderItems= new List<OrderItem>();
        //            decimal subTotal = 0; 


        //                     //ساخت اوردرایتم ها و بررسی قیمت کل
        //                       foreach (var item in dto.Items)
        //                       {
        //                           if (item.Quantity <= 0)
        //                           {
        //                               return ServiceResult<OrderDto>.Fail("تعداد محصول نامعتبر است");
        //                           }
        //                           var product= await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
        //                           if (product == null)
        //                           {
        //                               return ServiceResult<OrderDto>.Fail($"محصولی با شناسه {item.ProductId} یافت نشد");
        //                           }
        //                           if (!product.IsActive)
        //                           {
        //                               return ServiceResult<OrderDto>.Fail($"محصول «{product.Name}» در حال حاضر قابل سفارش نیست.");
        //                           }
        //                           if (product.StockQuantity < item.Quantity)
        //                           {
        //                               return ServiceResult<OrderDto>.Fail($"موجودی محصول «{product.Name}» کافی نیست.");
        //                           }





        //                           var orderitem = new OrderItem()
        //                           {
        //                               ProductId = product.Id,
        //                               ProductName = product.Name,
        //                               UnitPrice = product.Price,
        //                               Quantity = item.Quantity
        //                           };

        //                           orderItems.Add(orderitem);
        //                           subTotal += orderitem.TotalPrice;
        //                           product.StockQuantity -= item.Quantity;

        //                           await _unitOfWork.Repository<Product>().UpdateAsync(product);
        //                       }

        //                 //اعمال تخفیفم
        //                 decimal discountAmount = 0;
        //                 Coupon? coupon = null;

        //                 if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        //                 {
        //                     coupon = await _unitOfWork.Repository<Coupon>().GetFirstOrDefaultAsync(x => x.Code == dto.CouponCode);




        //                     if (coupon == null)
        //                     {
        //                         return ServiceResult<OrderDto>.Fail("کد تخفیف یافت نشد");
        //                     }

        //                     if (!coupon.IsActive)
        //                     {
        //                         return ServiceResult<OrderDto>.Fail("کد تخفیف غیرفعال است.");
        //                     }
        //                     if (coupon.ExpireDate < DateTime.UtcNow)
        //                     {
        //                         return ServiceResult<OrderDto>.Fail("اعتبار کد تخفیف به پایان رسیده است");
        //                     }
        //                     if (coupon.MinimumOrderAmount.HasValue && subTotal < coupon.MinimumOrderAmount.Value)
        //                     {
        //                         return ServiceResult<OrderDto>.Fail("سقف استفاده از کد تخفیف تکمیل شده است");
        //                     }


        //                     if(coupon.DiscountType== Domain.Enum.DiscountType.Percentage)
        //                     {
        //                         discountAmount = subTotal * coupon.Value / 100;
        //                     }
        //                     else
        //                     {
        //                         discountAmount=coupon.Value;
        //                     }


        //                     if (discountAmount > subTotal)
        //                     {
        //                         discountAmount = subTotal;
        //                     }



        //                     coupon.UsedCount++;
        //                     await _unitOfWork.Repository<Coupon>().UpdateAsync(coupon);
        //                 }
        //            //قیمت کلش با تخفیف
        //            var totalPrice=subTotal-discountAmount;


        //            //سفارش اصلیه
        //            var order = new Order
        //            {
        //                UserId = userId,
        //                SubTotal = subTotal,
        //                DiscountAmount = discountAmount,
        //                TotalPrice = totalPrice,
        //                Status = OrderStatus.Pending,
        //                CouponId = coupon?.Id,
        //                CreatedAt = DateTime.UtcNow,
        //            };
        //            foreach (var item in orderItems)
        //            {
        //                order.OrderItems.Add(item);
        //            }



        //            order.ShippingInfo = new ShippingInfo
        //            {
        //                FullName=dto.ShippingInfo.FullName,
        //                PhoneNumber=dto.ShippingInfo.PhoneNumber,
        //                 Address=dto.ShippingInfo.Address,
        //                 City=dto.ShippingInfo.City,
        //                 PostalCode=dto.ShippingInfo.PostalCode,
        //            };
        //            order.Payment = new Payment
        //            {
        //                Amount = totalPrice,
        //                Status = PaymentStatus.Paid,
        //                Method=PaymentMethod.Online,
        //                CreatedAt= DateTime.UtcNow,
        //            };
        //            order.OrderHistory.Add(new OrderHistory
        //            {
        //                OldStatus = null,
        //                NewStatus = OrderStatus.Pending,
        //                Description = "سفارش با موفقیت ثبت شد",
        //                ChangedBy = userId,
        //                CreatedAt= DateTime.UtcNow
        //            });

        //            await _unitOfWork.Repository<Order>().AddAsync(order);
        //            await _unitOfWork.SaveAsync();
        //            await _unitOfWork.CommitTransactionAsync();


        //            var result = new OrderDto
        //            {
        //                Id = order.Id,
        //                SubTotal = order.SubTotal,
        //                DiscountAmount = order.DiscountAmount,
        //                TotalPrice = order.TotalPrice,
        //                Status = order.Status,
        //                CreatedAt = DateTime.UtcNow,
        //                Items = order.OrderItems.Select(x => new OrderItemDto
        //                {
        //                    ProductId = x.ProductId,
        //                    ProductName= x.ProductName,
        //                    Quantity = x.Quantity,
        //                    UnitPrice = x.UnitPrice,
        //                }).ToList(),
        //            };

        //            return ServiceResult<OrderDto>.Ok(result, "سفارش با موفقیت ثبت شد.");
        //        }

        //        catch (Exception ex)
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();
        //            return ServiceResult<OrderDto>.Fail($"خطایی هنگام ثبت سفارش رخ داد: {ex.Message}");
        //        }
        //    }

        //    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int orderId, string userId)
        //    {
        //        var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId,
        //            query => query
        //            .Include(x => x.OrderItems)
        //            .Include(x => x.ShippingInfo)
        //            .Include(x => x.Payment)
        //            .Include(x => x.OrderHistory)
        //            );

        //        if (order == null)
        //        {
        //            return ServiceResult<OrderDto>.Fail("سفارشی یافت نشد");
        //        }

        //        var orderDto = new OrderDto
        //        {
        //            Id = order.Id,
        //            SubTotal = order.SubTotal,
        //            DiscountAmount = order.DiscountAmount,
        //            TotalPrice = order.TotalPrice,
        //            Status = order.Status,
        //            CreatedAt = order.CreatedAt,



        //            ShippingInfo = order.ShippingInfo == null
        //            ? null
        //            : new ShippingInfoDto
        //            {
        //                FullName = order.ShippingInfo.FullName,
        //                PhoneNumber = order.ShippingInfo.PhoneNumber,
        //                Address = order.ShippingInfo.Address,
        //                City = order.ShippingInfo.City,
        //                PostalCode = order.ShippingInfo.PostalCode,
        //                TrackingNumber = order.ShippingInfo.TrackingNumber,
        //                ShippedAt = order.ShippingInfo.ShippedAt,
        //                DeliveredAt = order.ShippingInfo.DeliveredAt,
        //            },


        //            Payment = order.Payment == null
        //            ? null
        //            : new PaymentDto
        //            {
        //                Amount = order.Payment.Amount,

        //                Method = order.Payment.Method,

        //                Status = order.Payment.Status,

        //                PaidAt = order.Payment.PaidAt,

        //                TransactionId = order.Payment.TransactionId
        //            },


        //            Items = order.OrderItems
        //            .Select(x => new OrderItemDto
        //            {
        //                ProductId = x.ProductId,

        //                ProductName = x.ProductName,

        //                Quantity = x.Quantity,

        //                UnitPrice = x.UnitPrice
        //            })
        //            .ToList(),


        //            History = order.OrderHistory
        //            .OrderBy(x => x.CreatedAt)
        //            .Select(x => new OrderHistoryDto
        //            {
        //                OldStatus = x.OldStatus,

        //                NewStatus = x.NewStatus,

        //                Description = x.Description,

        //                ChangedBy = x.ChangedBy,

        //                CreatedAt = x.CreatedAt
        //            })
        //            .ToList()
        //        };
        //        return ServiceResult<OrderDto>.Ok(orderDto);
        //    }


        //    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetAllOrdersAsync()
        //    {
        //        var orders = await _unitOfWork
        //            .Repository<Order>()
        //            .GetAllAsync(
        //                null,
        //                query => query
        //                    .Include(x => x.OrderItems)
        //                    .Include(x => x.Payment)
        //                    .Include(x => x.ShippingInfo)
        //                    .Include(x => x.OrderHistory)
        //            );



        //        var result = orders
        //            .OrderByDescending(x => x.CreatedAt)
        //            .Select(order => new OrderDto
        //            {
        //                Id = order.Id,

        //                SubTotal = order.SubTotal,

        //                DiscountAmount = order.DiscountAmount,

        //                TotalPrice = order.TotalPrice,

        //                Status = order.Status,

        //                CreatedAt = order.CreatedAt,



        //                ShippingInfo = order.ShippingInfo == null
        //                    ? null
        //                    : new ShippingInfoDto
        //                    {
        //                        FullName = order.ShippingInfo.FullName,

        //                        PhoneNumber = order.ShippingInfo.PhoneNumber,

        //                        Address = order.ShippingInfo.Address,

        //                        City = order.ShippingInfo.City,

        //                        PostalCode = order.ShippingInfo.PostalCode,

        //                        TrackingNumber = order.ShippingInfo.TrackingNumber,

        //                        ShippedAt = order.ShippingInfo.ShippedAt,

        //                        DeliveredAt = order.ShippingInfo.DeliveredAt
        //                    },



        //                Payment = order.Payment == null
        //                    ? null
        //                    : new PaymentDto
        //                    {
        //                        Amount = order.Payment.Amount,

        //                        Status = order.Payment.Status,

        //                        Method = order.Payment.Method,

        //                        TransactionId = order.Payment.TransactionId,

        //                        PaidAt = order.Payment.PaidAt
        //                    },



        //                Items = order.OrderItems
        //                    .Select(item => new OrderItemDto
        //                    {
        //                        ProductId = item.ProductId,

        //                        ProductName = item.ProductName,

        //                        Quantity = item.Quantity,

        //                        UnitPrice = item.UnitPrice
        //                    })
        //                    .ToList(),



        //                History = order.OrderHistory
        //                    .OrderBy(x => x.CreatedAt)
        //                    .Select(history => new OrderHistoryDto
        //                    {
        //                        OldStatus = history.OldStatus,

        //                        NewStatus = history.NewStatus,

        //                        Description = history.Description,

        //                        ChangedBy = history.ChangedBy,

        //                        CreatedAt = history.CreatedAt
        //                    })
        //                    .ToList()

        //            })
        //            .ToList();



        //        return ServiceResult<IEnumerable<OrderDto>>
        //            .Ok(result);
        //    }
        //    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(string userId)
        //    {
        //        var orders = await _unitOfWork
        //            .Repository<Order>()
        //            .GetAllAsync(
        //                x => x.UserId == userId,
        //                query => query
        //                    .Include(x => x.OrderItems)
        //                    .Include(x => x.Payment)
        //                    .Include(x => x.ShippingInfo)
        //                    .Include(x => x.OrderHistory)
        //            );



        //        var result = orders
        //            .OrderByDescending(x => x.CreatedAt)
        //            .Select(order => new OrderDto
        //            {
        //                Id = order.Id,

        //                SubTotal = order.SubTotal,

        //                DiscountAmount = order.DiscountAmount,

        //                TotalPrice = order.TotalPrice,

        //                Status = order.Status,

        //                CreatedAt = order.CreatedAt,



        //                ShippingInfo = order.ShippingInfo == null
        //                    ? null
        //                    : new ShippingInfoDto
        //                    {
        //                        FullName = order.ShippingInfo.FullName,

        //                        PhoneNumber = order.ShippingInfo.PhoneNumber,

        //                        Address = order.ShippingInfo.Address,

        //                        City = order.ShippingInfo.City,

        //                        PostalCode = order.ShippingInfo.PostalCode,

        //                        TrackingNumber = order.ShippingInfo.TrackingNumber,

        //                        ShippedAt = order.ShippingInfo.ShippedAt,

        //                        DeliveredAt = order.ShippingInfo.DeliveredAt
        //                    },



        //                Payment = order.Payment == null
        //                    ? null
        //                    : new PaymentDto
        //                    {
        //                        Amount = order.Payment.Amount,

        //                        Method = order.Payment.Method,

        //                        Status = order.Payment.Status,

        //                        TransactionId = order.Payment.TransactionId,

        //                        PaidAt = order.Payment.PaidAt
        //                    },



        //                Items = order.OrderItems
        //                    .Select(item => new OrderItemDto
        //                    {
        //                        ProductId = item.ProductId,

        //                        ProductName = item.ProductName,

        //                        Quantity = item.Quantity,

        //                        UnitPrice = item.UnitPrice
        //                    })
        //                    .ToList(),



        //                History = order.OrderHistory
        //                    .OrderBy(x => x.CreatedAt)
        //                    .Select(history => new OrderHistoryDto
        //                    {
        //                        OldStatus = history.OldStatus,

        //                        NewStatus = history.NewStatus,

        //                        Description = history.Description,

        //                        ChangedBy = history.ChangedBy,

        //                        CreatedAt = history.CreatedAt
        //                    })
        //                    .ToList()

        //            })
        //            .ToList();



        //        return ServiceResult<IEnumerable<OrderDto>>
        //            .Ok(result);
        //    }

        //    public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(int orderId,UpdateOrderStatusDto dto)
        //    {
        //        await _unitOfWork.BeginTransactionAsync();

        //        try
        //        {
        //            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(x => x.Id == orderId,
        //            query => query
        //                .Include(x => x.OrderItems)
        //                .Include(x => x.Payment)
        //                .Include(x => x.ShippingInfo)
        //                .Include(x => x.OrderHistory)
        //                );

        //            if (order == null)
        //            {
        //                await _unitOfWork.RollbackTransactionAsync();

        //                return ServiceResult<OrderDto>
        //                    .Fail("سفارش موردنظر یافت نشد.");
        //            }

        //            if (order.Status == dto.Status)
        //            {
        //                await _unitOfWork.RollbackTransactionAsync();

        //                return ServiceResult<OrderDto>
        //                    .Fail("وضعیت سفارش از قبل روی همین مقدار قرار دارد.");
        //            }





        //            var oldStatus = order.Status;

        //            order.Status = dto.Status;

        //            order.UpdatedAt = DateTime.UtcNow;




        //            if (order.Payment != null &&
        //                dto.Status == OrderStatus.Paid)
        //            {
        //                order.Payment.Status = PaymentStatus.Paid;

        //                order.Payment.PaidAt = DateTime.UtcNow;
        //            }

        //            if (order.ShippingInfo != null &&
        //                dto.Status == OrderStatus.Shipped)
        //            {
        //                order.ShippingInfo.ShippedAt = DateTime.UtcNow;
        //            }

        //            if (order.ShippingInfo != null &&
        //                dto.Status == OrderStatus.Delivered)
        //            {
        //                order.ShippingInfo.DeliveredAt = DateTime.UtcNow;
        //            }





        //            order.OrderHistory.Add(new OrderHistory
        //            {
        //                OldStatus = oldStatus,

        //                NewStatus = dto.Status,

        //                Description = dto.Description,

        //                ChangedBy = "Admin",

        //                CreatedAt = DateTime.UtcNow
        //            });

        //            await _unitOfWork.Repository<Order>().UpdateAsync(order);
        //            await _unitOfWork.SaveAsync();
        //            await _unitOfWork.CommitTransactionAsync();


        //            var result = new OrderDto
        //            {
        //                Id = order.Id,

        //                SubTotal = order.SubTotal,

        //                DiscountAmount = order.DiscountAmount,

        //                TotalPrice = order.TotalPrice,

        //                Status = order.Status,

        //                CreatedAt = order.CreatedAt,

        //                    ShippingInfo = order.ShippingInfo == null
        //                    ? null
        //                    : new ShippingInfoDto
        //                    {
        //                        FullName = order.ShippingInfo.FullName,

        //                        PhoneNumber = order.ShippingInfo.PhoneNumber,

        //                        Address = order.ShippingInfo.Address,

        //                        City = order.ShippingInfo.City,

        //                        PostalCode = order.ShippingInfo.PostalCode,

        //                        TrackingNumber = order.ShippingInfo.TrackingNumber,

        //                        ShippedAt = order.ShippingInfo.ShippedAt,

        //                        DeliveredAt = order.ShippingInfo.DeliveredAt
        //                    },

        //                    Payment = order.Payment == null
        //                    ? null
        //                    : new PaymentDto
        //                    {
        //                        Amount = order.Payment.Amount,

        //                        Method = order.Payment.Method,

        //                        Status = order.Payment.Status,

        //                        TransactionId = order.Payment.TransactionId,

        //                        PaidAt = order.Payment.PaidAt
        //                    },

        //                    Items = order.OrderItems
        //                    .Select(x => new OrderItemDto
        //                    {
        //                        ProductId = x.ProductId,

        //                        ProductName = x.ProductName,

        //                        Quantity = x.Quantity,

        //                        UnitPrice = x.UnitPrice
        //                    })
        //                    .ToList(),

        //                    History = order.OrderHistory
        //                    .OrderBy(x => x.CreatedAt)
        //                    .Select(x => new OrderHistoryDto
        //                    {
        //                        OldStatus = x.OldStatus,

        //                        NewStatus = x.NewStatus,

        //                        Description = x.Description,

        //                        ChangedBy = x.ChangedBy,

        //                        CreatedAt = x.CreatedAt
        //                    })
        //                    .ToList()
        //            };

        //            return ServiceResult<OrderDto>.Ok(result, "وضعیت سفارش با موفقیت بروزرسانی شد.");
        //        }
        //        catch (Exception)
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();

        //            return ServiceResult<OrderDto>.Fail("هنگام بروزرسانی وضعیت سفارش خطایی رخ داد. لطفاً مجدداً تلاش کنید.");
        //        }
        //    }


        //    public async Task<ServiceResult<OrderDto>> CancelOrderAsync(int orderId,string userId)
        //    {
        //        await _unitOfWork.BeginTransactionAsync();

        //        try
        //        {
        //            var order = await _unitOfWork.Repository<Order>() .GetFirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId,
        //            query => query
        //                .Include(x => x.OrderItems)
        //                .Include(x => x.Payment)
        //                .Include(x => x.ShippingInfo)
        //                .Include(x => x.OrderHistory)
        //                .Include(x => x.OrderItems)
        //                .ThenInclude(x => x.Product)
        //            );


        //            if (order == null)
        //            {
        //                await _unitOfWork.RollbackTransactionAsync();
        //                return ServiceResult<OrderDto>.Fail("سفارش موردنظر یافت نشد.");
        //            }



        //            if (order.Status != OrderStatus.Pending &&
        //                order.Status != OrderStatus.Paid)
        //            {
        //                await _unitOfWork.RollbackTransactionAsync();
        //                return ServiceResult<OrderDto>.Fail(
        //                        "این سفارش قابل لغو نیست."
        //                    );
        //            }



        //            var oldStatus = order.Status;

        //            order.Status = OrderStatus.Cancelled;

        //            order.UpdatedAt = DateTime.UtcNow;



        //            foreach (var item in order.OrderItems)
        //            {
        //                item.Product.StockQuantity += item.Quantity;
        //                await _unitOfWork.Repository<Product>().UpdateAsync(item.Product);
        //            }





        //            order.OrderHistory.Add(new OrderHistory
        //            {
        //                OldStatus = oldStatus,

        //                NewStatus = OrderStatus.Cancelled,

        //                Description = "سفارش توسط مشتری لغو شد.",

        //                ChangedBy = userId,

        //                CreatedAt = DateTime.UtcNow
        //            });



        //            await _unitOfWork.Repository<Order>().UpdateAsync(order);
        //            await _unitOfWork.SaveAsync();
        //            await _unitOfWork.CommitTransactionAsync();



        //            var result = new OrderDto
        //            {
        //                Id = order.Id,

        //                SubTotal = order.SubTotal,

        //                DiscountAmount = order.DiscountAmount,

        //                TotalPrice = order.TotalPrice,

        //                Status = order.Status,

        //                CreatedAt = order.CreatedAt,


        //                ShippingInfo = order.ShippingInfo == null
        //                    ? null
        //                    : new ShippingInfoDto
        //                    {
        //                        FullName = order.ShippingInfo.FullName,

        //                        PhoneNumber = order.ShippingInfo.PhoneNumber,

        //                        Address = order.ShippingInfo.Address,

        //                        City = order.ShippingInfo.City,

        //                        PostalCode = order.ShippingInfo.PostalCode,

        //                        TrackingNumber = order.ShippingInfo.TrackingNumber,

        //                        ShippedAt = order.ShippingInfo.ShippedAt,

        //                        DeliveredAt = order.ShippingInfo.DeliveredAt
        //                    },


        //                Payment = order.Payment == null
        //                    ? null
        //                    : new PaymentDto
        //                    {
        //                        Amount = order.Payment.Amount,

        //                        Status = order.Payment.Status,

        //                        Method = order.Payment.Method,

        //                        TransactionId = order.Payment.TransactionId,

        //                        PaidAt = order.Payment.PaidAt
        //                    },


        //                Items = order.OrderItems
        //                    .Select(x => new OrderItemDto
        //                    {
        //                        ProductId = x.ProductId,

        //                        ProductName = x.ProductName,

        //                        Quantity = x.Quantity,

        //                        UnitPrice = x.UnitPrice
        //                    })
        //                    .ToList(),


        //                History = order.OrderHistory
        //                    .OrderBy(x => x.CreatedAt)
        //                    .Select(x => new OrderHistoryDto
        //                    {
        //                        OldStatus = x.OldStatus,

        //                        NewStatus = x.NewStatus,

        //                        Description = x.Description,

        //                        ChangedBy = x.ChangedBy,

        //                        CreatedAt = x.CreatedAt
        //                    })
        //                    .ToList()
        //            };


        //            return ServiceResult<OrderDto>.Ok(result, "سفارش با موفقیت لغو شد.");
        //        }
        //        catch (Exception)
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();

        //            return ServiceResult<OrderDto>.Fail("هنگام لغو سفارش خطایی رخ داد. لطفاً دوباره تلاش کنید.");
        //        }
        //    }

    }
}
