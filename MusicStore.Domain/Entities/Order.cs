using MusicStore.Domain.common;
  using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        // مجموع قیمت محصولات قبل از تخفیف
        public decimal SubTotal { get; set; }

        // مبلغ تخفیف اعمال شده
        public decimal DiscountAmount { get; set; }

        // مبلغ نهایی قابل پرداخت
        public decimal TotalPrice { get; set; }



        public OrderStatus Status { get; set; }
            = OrderStatus.Pending;

        // کد تخفیف
        public int? CouponId { get; set; }


        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        public DateTime? UpdatedAt { get; set; }





        public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();

        public Payment? Payment { get; set; }

        public ShippingInfo? ShippingInfo { get; set; }

        public Coupon? Coupon { get; set; }

        public ICollection<OrderHistory> OrderHistory { get; set; }= new List<OrderHistory>();

    }
}

