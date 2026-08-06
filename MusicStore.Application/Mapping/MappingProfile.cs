using AutoMapper;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.DTOs.OrderItem;
using MusicStore.Domain.Entities;

namespace MusicStore.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>().ReverseMap();

        CreateMap<OrderItem, OrderItemDto>().ReverseMap();

        CreateMap<ShippingInfo, ShippingInfoDto>().ReverseMap();

        CreateMap<Payment, PaymentDto>().ReverseMap();

        CreateMap<OrderHistory, OrderHistoryDto>().ReverseMap();
    }
}