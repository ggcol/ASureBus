using System.Reflection;
using System.Text.Json;
using ASureBus.Abstractions;
using ASureBus.Core.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.IntegrationTests.TestConfigurations;

public class WithAsbHostAndCheckService
{
    private readonly IHost _host;
    protected IMessagingContext Context => Get.ServiceFromHost<IMessagingContext>(_host);
    protected CheckService CheckService => Get.ServiceFromHost<CheckService>(_host);

    protected WithAsbHostAndCheckService()
    {
        var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        if (string.IsNullOrEmpty(serviceBusConnectionString))
        {
            var raw = File.ReadAllText(Path.Combine(".", "integrationTestsSettings.json"));
            var settings = JsonSerializer.Deserialize<IntegrationTestsSettings>(raw);
            serviceBusConnectionString = settings?.ServiceBusConnectionString;
        }

        var thisAssembly = Assembly.GetExecutingAssembly();
        var hostBuilder = Host
            .CreateDefaultBuilder()
            .UseAsb(opt => { opt.ConnectionString = serviceBusConnectionString; }, thisAssembly)
            .ConfigureServices(services => { services.AddSingleton<CheckService>(); });

        _host = hostBuilder.Build();
    }

    protected void RunHost()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _host.RunAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    protected async Task StopHost()
    {
        await _host.StopAsync().ConfigureAwait(false);
    }
}