using ASureBus.Core.MessageProcessing.LockHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling;

internal sealed class MessageLockObserver : IMessageLockObserver
{
    private readonly IDictionary<object, MessageLockObservable> _shelf
        = new Dictionary<object, MessageLockObservable>();

    public void RenewOnExpiration(ProcessMessageEventArgs args)
    {
        var obs = new MessageLockObservable(args);

        obs.Expired += async (_, _) =>
        {
            await args.RenewMessageLockAsync(args.Message).ConfigureAwait(false);
            _shelf.Remove(args.Message);
        };

        _shelf.Add(args.Message, obs);
    }
}