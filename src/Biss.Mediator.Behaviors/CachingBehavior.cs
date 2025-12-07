using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that provides caching for query requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _defaultCacheDuration;

    public CachingBehavior(
        IMemoryCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        TimeSpan? defaultCacheDuration = null)
    {
        _cache = cache;
        _logger = logger;
        _defaultCacheDuration = defaultCacheDuration ?? TimeSpan.FromMinutes(5);
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache query requests
        if (!IsQueryRequest(request))
        {
            return await next(request, cancellationToken);
        }

        var cacheKey = GenerateCacheKey(request);
        
        if (_cache.TryGetValue(cacheKey, out var cachedResult) && cachedResult is Result<TResponse> cached)
        {
            _logger.LogDebug("Cache hit for request {RequestType} with key {CacheKey}", 
                typeof(TRequest).Name, cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss for request {RequestType} with key {CacheKey}", 
            typeof(TRequest).Name, cacheKey);

        var result = await next(request, cancellationToken);

        if (result.IsSuccess)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultCacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            _cache.Set(cacheKey, result, cacheOptions);
            
            _logger.LogDebug("Cached result for request {RequestType} with key {CacheKey}", 
                typeof(TRequest).Name, cacheKey);
        }

        return result;
    }

    private static bool IsQueryRequest(TRequest request)
    {
        // Check if the request implements IQuery<TResponse>
        var requestType = request.GetType();
        return requestType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IQuery");
    }

    private static string GenerateCacheKey(TRequest request)
    {
        // Create a cache key based on the request type and its properties
        var requestType = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return $"{requestType}:{requestJson.GetHashCode()}";
    }
}
