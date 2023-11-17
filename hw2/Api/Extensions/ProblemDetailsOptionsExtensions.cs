using Domain.Exceptions;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Api.Extensions;

public static class ProblemDetailsOptionsExtensions
{
    public static void MapFluentValidationException(this ProblemDetailsOptions options)
    {
        options.Map<ValidationException>((context, exception) =>
        {
            var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());

            return factory.CreateValidationProblemDetails(context, errors);
        });
    }

    public static void MapProductNotFoundException(this ProblemDetailsOptions options)
    {
        options.Map<ProductNotFoundException>((context, exception) =>
            context.RequestServices.GetRequiredService<ProblemDetailsFactory>()
                .CreateProblemDetails(context, StatusCodes.Status404NotFound, detail: exception.Message));
    }
}