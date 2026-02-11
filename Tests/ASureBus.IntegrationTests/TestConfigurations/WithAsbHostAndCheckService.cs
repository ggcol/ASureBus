using System.Reflection;
using System.Text.Json;
using ASureBus.Abstractions;
using ASureBus.Core.DI;
using ASureBus.IntegrationTests.Settings;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.IntegrationTests.TestConfigurations;

public abstract class WithAsbHostAndCheckService
{
    private IHost? _host;
    private bool _isDisposed;
    private readonly string? _serviceBusConnectionString;

    protected readonly IHostBuilder HostBuilder;
    protected IMessagingContext Context => Get.ServiceFromHost<IMessagingContext>(_host!);
    protected CheckService CheckService => Get.ServiceFromHost<CheckService>(_host!);

    protected internal WithAsbHostAndCheckService()
    {
        _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        if (string.IsNullOrEmpty(_serviceBusConnectionString))
        {
            var raw = File.ReadAllText(Path.Combine("./Settings/", "integrationTestsSettings.json"));
            var settings = JsonSerializer.Deserialize<IntegrationTestsSettings>(raw);
            _serviceBusConnectionString = settings?.ServiceBusConnectionString;
        }

        CleanServiceBusInfrastructure().GetAwaiter().GetResult();

        var thisAssembly = Assembly.GetExecutingAssembly();
        HostBuilder = Host
            .CreateDefaultBuilder()
            .UseAsb(opt => { opt.ConnectionString = _serviceBusConnectionString; }, thisAssembly)
            .ConfigureServices(services => { services.AddSingleton<CheckService>(); });
    }

    protected void RunHost()
    {
        _host = HostBuilder.Build();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _host.RunAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    protected async Task StopHost()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        await _host!.StopAsync().ConfigureAwait(false);
        _host.Dispose();
    }

    private async Task CleanServiceBusInfrastructure()
    {
        var admClient = new ServiceBusAdministrationClient(_serviceBusConnectionString);

        await foreach (var queue in admClient.GetQueuesAsync())
        {
            await admClient.DeleteQueueAsync(queue.Name).ConfigureAwait(false);
        }

        await foreach (var topic in admClient.GetTopicsAsync())
        {
            await admClient.DeleteTopicAsync(topic.Name).ConfigureAwait(false);
        }
    }
}