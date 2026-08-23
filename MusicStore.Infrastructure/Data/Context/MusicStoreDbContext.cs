using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Data.Context;

public class MusicStoreDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public MusicStoreDbContext(
        DbContextOptions<MusicStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ShippingInfo> ShippingInfos => Set<ShippingInfo>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<StockHistory> StockHistories => Set<StockHistory>();

    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewReply> ReviewReplies => Set<ReviewReply>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MusicStoreDbContext).Assembly);
    }
}