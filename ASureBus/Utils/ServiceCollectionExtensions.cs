using Microsoft.Extensions.DependencyInjection;

namespace ASureBus.Utils;

internal static class ServiceCollectionExtensions
{
    internal static void RemoveService<T>(this IServiceCollection services)
    {
        if (services.Count <= 0) return;

        var service = services.SingleOrDefault(x => x.ServiceType == typeof(T));
        if (service is not null)
        {
            services.Remove(service);
        }
    }

    internal static bool TryGetSingleOrDefault<T>(this IServiceCollection services, out T service)
    {
        service = default!;
        if (services.Count <= 0) return false;

        var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(T));
        if (descriptor is null) return false;

        service = (T)descriptor.ImplementationInstance!;
        return true;
    }
}