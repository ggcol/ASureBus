using ASureBus.Core.Behaviours;
using ASureBus.Core.MessageProcessing.LockHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling;

internal sealed class MessageLockObserver : Observer<MessageLockObservable>, IMessageLockObserver
{
    public void RenewOnExpiration(ProcessMessageEventArgs args)
    {
        var obs = new MessageLockObservable(args);

        Observe(args.Message, obs,  async void (_, _) =>
        {
            await args.RenewMessageLockAsync(args.Message).ConfigureAwait(false);
        });
    }
}