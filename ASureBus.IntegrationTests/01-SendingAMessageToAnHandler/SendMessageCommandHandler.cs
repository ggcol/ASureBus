using ASureBus.Abstractions;

namespace ASureBus.IntegrationTests._01_SendingAMessageToAnHandler;

public class SendMessageCommandHandler(CheckService checkService) : IHandleMessage<SendingAMessageToHandlerCommand>
{
    public Task Handle(SendingAMessageToHandlerCommand messageToHandler, IMessagingContext context, CancellationToken cancellationToken)
    {
        checkService.Acknowledged = true;
        return Task.CompletedTask;
    }
}