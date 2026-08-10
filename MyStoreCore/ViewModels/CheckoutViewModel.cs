
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MusicStore.Application.DTOs.Cart;
using MusicStore.Application.DTOs.Order;

namespace MyStoreCore.ViewModels.Order
{
    public class CheckoutViewModel
    {
        
        public CartDto? Cart { get; set; }

        public ShippingInfoDto ShippingInfo { get; set; } = new();

        public string? CouponCode { get; set; }

        [BindNever]
        public decimal SubTotal { get; set; }

        [BindNever]
        public decimal DiscountAmount { get; set; }

        [BindNever]
        public decimal FinalPrice { get; set; }
    }
}

