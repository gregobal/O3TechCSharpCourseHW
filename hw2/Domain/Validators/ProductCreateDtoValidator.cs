using Domain.Dtos;
using FluentValidation;

namespace Domain.Validators;

public sealed class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).SetValidator(new PriceValidator());
        RuleFor(x => x.Weight).GreaterThan(0);
    }
}