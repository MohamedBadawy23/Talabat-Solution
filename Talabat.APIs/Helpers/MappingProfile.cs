using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;
using UserAddress =  Talabat.Core.Entities.Identity.Address;
using OrderAddress =  Talabat.Core.Entities.Order.Address;
using Talabat.Core.Entities.Order;

namespace Talabat.APIs.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.ProductBrand, O => O.MapFrom(s => s.ProductBrand.Name))
                .ForMember(d => d.ProductType, O => O.MapFrom(s => s.ProductType.Name))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());

            CreateMap<UserAddress, AddressDto>().ReverseMap();
            CreateMap<OrderAddress, AddressDto>().ReverseMap()
                .ForMember(d => d.FirstName, O => O.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, O => O.MapFrom(s => s.LastName))
                ;


            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost))
                ;

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, O => O.MapFrom(s => s.Product.ProductId))
                .ForMember(d => d.ProductName, O => O.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.PictureUrl, O => O.MapFrom(s => s.Product.PictureUrl))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>())
                ;

            CreateMap<CustomerBasket, CustomerBasketDto>().ReverseMap();
        }


    }
}
