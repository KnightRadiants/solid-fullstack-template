using AutoMapper;
using SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Dishes.Dtos;

public class DishesProfile : Profile
{
    public DishesProfile()
    {
        CreateMap<CreateDishCommand, Dish>();
        CreateMap<Dish, DishDto>();
    }
}
