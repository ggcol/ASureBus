using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playground.Samples._12_GenericTypeMessages;

public class GenericTypeMessagesInitJob(
    IHostApplicationLifetime lifetime,
    ILogger<GenericTypeMessagesInitJob> logger,
    IMessagingContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new AGenericMessage<string>("Hello, World!"), cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Sent AMessage<string>");
        
        await context.Send(new AGenericMessage<int>(42), cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Sent AMessage<int>");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping...");
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}

public record AGenericMessage<T>(T Data) : IAmACommand;
public class AMessageStringFlavourHandler(ILogger<AMessageStringFlavourHandler> logger)
    : IHandleMessage<AGenericMessage<string>>
{
    public Task Handle(AGenericMessage<string> genericMessage, IMessagingContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling message: {MessageData}", genericMessage.Data);
        return Task.CompletedTask;
    }
}

public class AMessageIntFlavourHandler(ILogger<AMessageIntFlavourHandler> logger)
    : IHandleMessage<AGenericMessage<int>>
{
    public Task Handle(AGenericMessage<int> genericMessage, IMessagingContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling message: {MessageData}", genericMessage.Data);
        return Task.CompletedTask;
    }
}