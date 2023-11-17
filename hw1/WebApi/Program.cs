using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Hellang.Middleware.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (_, _) => false;
    options.Map<DomainException>((ctx, ex) =>
        ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>()
            .CreateProblemDetails(ctx, StatusCodes.Status400BadRequest, detail: ex.Message));
});

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Version = "v1",
    Title = "Homework 1"
}));

services.AddScoped<ISaleHistoryRepository, SaleHistoryRepository>();
services.AddScoped<IMonthFactorRepository, MonthFactorRepository>();

services.AddScoped<ISaleAnalyticsService, SaleAnalyticsService>();

services.Configure<RouteOptions>(options => options.LowercaseUrls = true);


var app = builder.Build();

app.UseProblemDetails();

app.UseSwagger();

app.MapControllers();

app.UseSwaggerUI();

app.Run();