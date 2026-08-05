using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice =>
            UnitPrice * Quantity;
    }
}
