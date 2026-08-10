
using AutoMapper;
using MusicStore.Application.DTOs.Coupon;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.DTOs.OrderItem;
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
        }
    }
}

