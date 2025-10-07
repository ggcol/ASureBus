using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling;

internal interface IMessageLockObserver
{
    internal void RenewOnExpiration(ProcessMessageEventArgs args);
}