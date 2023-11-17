using Domain.Dtos;
using FluentValidation;

namespace Domain.Validators;

public sealed class UpdatePriceDtoValidator : AbstractValidator<UpdatePriceDto>
{
    public UpdatePriceDtoValidator()
    {
        RuleFor(x => x.Price).SetValidator(new PriceValidator());
    }
}