using System.Text.Json;
using ASureBus.Core.DI;
using ASureBus.IntegrationTests.Settings;
using Azure.Storage.Blobs;

namespace ASureBus.IntegrationTests.TestConfigurations;

public abstract class WithAsbHostCheckServiceAndHeavies : WithAsbHostAndCheckService
{
    private readonly string? _storageAccountConnectionString;
    private readonly string _container = $"heavies-{DateTime.UtcNow:yyyyMMddHHmmss}";
    
    protected internal WithAsbHostCheckServiceAndHeavies()
    {
        _storageAccountConnectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
        
        if (string.IsNullOrEmpty(_storageAccountConnectionString))
        {
            var raw = File.ReadAllText(Path.Combine("./Settings/", "heaviesIntegrationTestsSettings.json"));
            var settings = JsonSerializer.Deserialize<HeaviesIntegrationTestsSettings>(raw);
            _storageAccountConnectionString = settings?.StorageAccountConnectionString;
        }
        
        HostBuilder.UseHeavyProps(opt =>
        {
            opt.ConnectionString = _storageAccountConnectionString;
            opt.Container = _container;
        });
        
        MakeContainer().GetAwaiter().GetResult();
    }
    
    private async Task MakeContainer()
    {
        var containerClient = new BlobContainerClient(_storageAccountConnectionString, _container);
        await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
    }
    
    protected void CleanHeaviesContainer()
    {
        var containerClient = new BlobContainerClient(_storageAccountConnectionString, _container);
        containerClient.DeleteIfExists();
    }
}