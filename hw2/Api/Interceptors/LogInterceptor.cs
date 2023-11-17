using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api.Interceptors;

public class LogInterceptor : Interceptor
{
    private readonly ILogger<LogInterceptor> _logger;

    public LogInterceptor(ILogger<LogInterceptor> logger)
    {
        _logger = logger;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Starting receiving call. Type/Method: {Type} / {Method}. Request: {Request}",
            MethodType.Unary, context.Method, request);

        var response = continuation(request, context);

        _logger.LogInformation("Receiving call completed. Type/Method: {Type} / {Method}. Response: {Response}",
            MethodType.Unary, context.Method, response.Result);

        return response;
    }
}