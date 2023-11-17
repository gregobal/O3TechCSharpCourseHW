using FluentValidation;

namespace Domain.Validators;

internal sealed class PriceValidator : AbstractValidator<decimal>
{
    public PriceValidator()
    {
        RuleFor(x => x).GreaterThanOrEqualTo(0)
            .Configure(options => options.SetDisplayName("Price"));
    }
}