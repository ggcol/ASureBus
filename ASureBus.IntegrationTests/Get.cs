using Microsoft.Extensions.Hosting;

namespace ASureBus.IntegrationTests;

internal static class Get
{
    internal static T ServiceFromHost<T>(IHost host)
    {
        return (T)host.Services.GetService(typeof(T))!;
    }
}