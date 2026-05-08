using FluentValidation;

namespace SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;

public class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
{
    public CreateDishCommandValidator()
    {
        RuleFor(dto => dto.Name)
            .Length(3, 100);

        RuleFor(dto => dto.Price)
            .GreaterThan(0);

        RuleFor(dto => dto.KiloCalories)
            .GreaterThan(0);
    }
}
