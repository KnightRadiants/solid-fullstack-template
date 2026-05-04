using AutoMapper;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Dishes.Dtos;

public class DishesProfile : Profile
{
    public DishesProfile()
    {
        CreateMap<Dish, DishDto>();
    }
}
