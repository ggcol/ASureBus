using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling;

internal interface IMessageLockObserver
{
    void RenewOnExpiration(ProcessMessageEventArgs args);
}