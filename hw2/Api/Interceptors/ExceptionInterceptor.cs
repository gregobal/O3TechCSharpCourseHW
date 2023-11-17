using Domain.Exceptions;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (AggregateException aggregateException)
        {
            if (aggregateException.InnerException is { } innerException)
                switch (innerException)
                {
                    case ValidationException:
                        throw new RpcException(new Status(StatusCode.InvalidArgument, innerException.Message));

                    case ProductNotFoundException:
                        throw new RpcException(new Status(StatusCode.NotFound, innerException.Message));
                }

            throw;
        }
    }
}