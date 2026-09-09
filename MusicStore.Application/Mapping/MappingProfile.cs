using AutoMapper;
using MusicStore.Application.DTOs.Coupon;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.DTOs.OrderItem;
using MusicStore.Application.DTOs.Product;
using MusicStore.Application.DTOs.ProductDiscount;
using MusicStore.Application.DTOs.Stock;
using MusicStore.Application.DTOs.Wishlist;
using MusicStore.Domain.Entities;

namespace MusicStore.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(
                    dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems)
                )
                .ForMember(
                    dest => dest.ShippingInfo,
                    opt => opt.MapFrom(src => src.ShippingInfo)
                )
                .ForMember(
                    dest => dest.Payment,
                    opt => opt.MapFrom(src => src.Payment)
                )
                .ForMember(
                    dest => dest.History,
                    opt => opt.MapFrom(src => src.OrderHistory)
                )
                .ReverseMap();

            CreateMap<OrderItem, OrderItemDto>()
                .ReverseMap();

            CreateMap<ShippingInfo, ShippingInfoDto>()
                .ReverseMap();

            CreateMap<Payment, PaymentDto>()
                .ReverseMap();

            CreateMap<OrderHistory, OrderHistoryDto>()
                .ReverseMap();

            CreateMap<Coupon, CouponDto>()
                .ReverseMap();

            CreateMap<CreateCouponDto, Coupon>();

            CreateMap<StockHistory, StockHistoryDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name)
                )
                .ForMember(
                    dest => dest.SKU,
                    opt => opt.MapFrom(src => src.Product.SKU)
                )
                .ForMember(
                    dest => dest.CreatedByName,
                    opt => opt.Ignore()
                );


     
            CreateMap<Product, ProductDto>()
                .ForMember(
                    dest => dest.MainImageUrl,
                    opt => opt.MapFrom(
                        src => src.Images
                            .FirstOrDefault(x => x.IsMain) != null
                                ? src.Images.First(x => x.IsMain).ImageUrl
                                : null
                    )
                )
                .ForMember(
                    dest => dest.ImageCount,
                    opt => opt.MapFrom(src => src.Images.Count)
                )
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null
                        ? src.Category.Name
                        : "")
                )
                .ForMember(
                    dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand != null
                        ? src.Brand.Name
                        : "")
                );
            
                CreateMap<Wishlist, WishlistDto>();
            


                CreateMap<ProductDiscount, ProductDiscountDto>()
                    .ForMember(
                        dest => dest.SKU,
                        opt => opt.MapFrom(src => src.Product.SKU)
                    )
                    .ForMember(
                        dest => dest.ProductName,
                        opt => opt.MapFrom(src => src.Product.Name)
                    );
                
                            CreateMap<CreateProductDiscountDto, ProductDiscount>();
                
                            CreateMap<UpdateProductDiscountDto, ProductDiscount>();
                

        }
    }
}