using AutoMapper;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Restaurants.Dtos;

public class RestaurantsProfile : Profile
{
    public RestaurantsProfile()
    {
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(restaurantDto => restaurantDto.City,
                memberOptions
                    => memberOptions.MapFrom(src => src.Address == null ? null : src.Address.City))
            .ForMember(restaurantDto => restaurantDto.PostalCode,
                memberOptions
                    => memberOptions.MapFrom(src => src.Address == null ? null : src.Address.PostalCode))
            .ForMember(restaurantDto => restaurantDto.Street,
                memberOptions
                    => memberOptions.MapFrom(src => src.Address == null ? null : src.Address.Street))
            .ForMember(restaurantDto => restaurantDto.Dishes,
            memberOptions
                => memberOptions.MapFrom(src => src.Dishes));
    }
}
