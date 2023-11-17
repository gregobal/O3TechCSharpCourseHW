using Api.Extensions;
using Api.GrpcServices;
using Api.Interceptors;
using Api.Utils;
using Domain.Dtos;
using Domain.Interfaces;
using Domain.Services;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

const string grpcApiGroupVersion = "v1";
const string aspApiGroupVersion = "v2";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionInterceptor>();
    options.Interceptors.Add<LogInterceptor>();
}).AddJsonTranscoding();
services.AddGrpcReflection();

services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (_, _) => false;
    options.MapFluentValidationException();
    options.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;
    options.MapProductNotFoundException();
});

services.AddControllers();

services.AddEndpointsApiExplorer();
services.AddGrpcSwagger();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(grpcApiGroupVersion,
        new OpenApiInfo { Title = "API V1 (gRPC transcoding)" });
    options.SwaggerDoc(aspApiGroupVersion,
        new OpenApiInfo { Title = "API V2 (Asp.net)" });
    options.DocInclusionPredicate((groupVersion, apiDesc) =>
        apiDesc.RelativePath != null && apiDesc.RelativePath.Contains(groupVersion));
});

services.AddValidatorsFromAssemblyContaining<ProductCreateDto>();

services.AddAutoMapper(typeof(AutoMapperProfile));

services.AddSingleton<IProductRepository, ProductInMemoryRepository>();

services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", grpcApiGroupVersion);
        options.SwaggerEndpoint("/swagger/v2/swagger.json", aspApiGroupVersion);
        options.RoutePrefix = string.Empty;
    });
}

app.MapGrpcService<ProductApiGrpcService>();

app.MapGet("",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapControllers();

app.Run();