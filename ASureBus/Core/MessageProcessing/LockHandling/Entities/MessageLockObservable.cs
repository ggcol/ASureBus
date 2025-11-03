using ASureBus.Abstractions.Behaviours;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling.Entities;

internal sealed class MessageLockObservable(ProcessMessageEventArgs args)
    : ObservableExpirable(args.Message.LockedUntil 
                          - DateTimeOffset.UtcNow 
                          - TimeSpan.FromSeconds(AsbConfiguration.MessageLockOptions.MessageLockRenewalPreemptiveThresholdInSeconds));