using System.Net;
using Grpc.Core;
using Grpc.Core.Interceptors;
using HomeworkApp.Dal.Repositories.Interfaces;

namespace HomeworkApp.Interceptors;

public class RateLimitingUnaryInterceptor : Interceptor
{
    private const string IpAddressHeaderKey = "X-R256-USER-IP";
    
    private readonly IRateLimiterRepository _repository;

    public RateLimitingUnaryInterceptor(IRateLimiterRepository repository)
    {
        _repository = repository;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var userIpEntry = context.RequestHeaders.Get(IpAddressHeaderKey);

        if (!IPAddress.TryParse(userIpEntry?.Value, out _))
            return await continuation(request, context);

        var isAllowed = await _repository.IsAllowed(userIpEntry.Value, context.CancellationToken);

        if (isAllowed) return await continuation(request, context);

        throw new RpcException(new Status(StatusCode.ResourceExhausted, "Too many requests"));
    }
}