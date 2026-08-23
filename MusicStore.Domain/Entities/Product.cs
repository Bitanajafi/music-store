using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MusicStore.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string SKU { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }

        public int CategoryId { get; set; }

        public int BrandId { get; set; }

        public Category Category { get; set; } = null!;

        public Brand Brand { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Wishlist> Wishlists { get; set; }= new List<Wishlist>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<StockHistory> StockHistories { get; set; }= new List<StockHistory>();

    }
}
