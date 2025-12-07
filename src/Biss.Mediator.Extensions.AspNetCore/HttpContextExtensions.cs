using Biss.Mediator.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.Mediator.Extensions.AspNetCore;

/// <summary>
/// Extension methods for accessing Mediator from HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the Mediator instance from the current HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The Mediator instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Mediator is not registered in the service collection.</exception>
    public static IMediator GetMediator(this HttpContext httpContext)
    {
        if (httpContext == null)
            throw new ArgumentNullException(nameof(httpContext));

        var mediator = httpContext.RequestServices.GetService<IMediator>();
        
        if (mediator == null)
            throw new InvalidOperationException(
                "IMediator is not registered. Please call AddMediator or AddMediatorWithAspNetCore in your service configuration.");

        return mediator;
    }

    /// <summary>
    /// Gets the Mediator instance from the current HTTP context, or null if not available.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The Mediator instance, or null if not registered.</returns>
    public static IMediator? GetMediatorOrNull(this HttpContext httpContext)
    {
        if (httpContext == null)
            throw new ArgumentNullException(nameof(httpContext));

        return httpContext.RequestServices.GetService<IMediator>();
    }
}

