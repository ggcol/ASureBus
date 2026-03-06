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

    protected async Task RunHost()
    {
        _host = HostBuilder.Build();
        await _host.StartAsync().ConfigureAwait(false);
    }

    protected async Task StopHost()
    {
        if (_isDisposed || _host is null) return;

        _isDisposed = true;
        
        try
        {
            await _host.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            // Host may not have fully started — swallow stop errors
        }
        finally
        {
            _host.Dispose();
        }
    }

    private async Task CleanServiceBusInfrastructure()
    {
        var admClient = new ServiceBusAdministrationClient(_serviceBusConnectionString);

        var deletedAny = false;

        await foreach (var topic in admClient.GetTopicsAsync())
        {
            await admClient.DeleteTopicAsync(topic.Name).ConfigureAwait(false);
            deletedAny = true;
        }

        await foreach (var queue in admClient.GetQueuesAsync())
        {
            await admClient.DeleteQueueAsync(queue.Name).ConfigureAwait(false);
            deletedAny = true;
        }

        if (!deletedAny) return;

        // Azure Service Bus deletes are asynchronous; entities may remain in a
        // transitional state for a few seconds. Wait until the namespace is clean
        // before allowing host startup to recreate them.
        const int maxWaitAttempts = 15;
        const int delayMs = 2000;

        for (var i = 0; i < maxWaitAttempts; i++)
        {
            await Task.Delay(delayMs).ConfigureAwait(false);

            var hasQueues = false;
            await foreach (var _ in admClient.GetQueuesAsync())
            {
                hasQueues = true;
                break;
            }

            var hasTopics = false;
            await foreach (var _ in admClient.GetTopicsAsync())
            {
                hasTopics = true;
                break;
            }

            if (!hasQueues && !hasTopics) return;
        }
    }
}