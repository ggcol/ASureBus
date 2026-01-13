using ASureBus.Abstractions.Behaviours;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling.Entities;

internal sealed class MessageLockObservable : ObservableExpirable
{
    private static readonly TimeSpan DefaultLockDuration = TimeSpan.FromSeconds(30);
    
    public MessageLockObservable(ProcessMessageEventArgs args, DateTimeOffset? startTime = null)
        : base(CalculateExpirationTime(args), startTime ?? DateTimeOffset.UtcNow)
    {
    }
    
    public MessageLockObservable(TimeSpan lockRenewalDuration, DateTimeOffset startTime)
        : base(CalculateExpirationTimeFromDuration(lockRenewalDuration), startTime)
    {
    }

    private static TimeSpan? CalculateExpirationTime(ProcessMessageEventArgs args)
    {
        var timeUntilLockExpires = args.Message.LockedUntil - DateTimeOffset.UtcNow
                                   - TimeSpan.FromSeconds(AsbConfiguration.MessageLockOptions
                                       .MessageLockRenewalPreemptiveThresholdInSeconds);

        return timeUntilLockExpires <= TimeSpan.Zero 
            ? CalculateExpirationTimeFromDuration(DefaultLockDuration) 
            : timeUntilLockExpires;
    }
    
    private static TimeSpan? CalculateExpirationTimeFromDuration(TimeSpan lockDuration)
    {
        var thresholdInSeconds = AsbConfiguration.MessageLockOptions.MessageLockRenewalPreemptiveThresholdInSeconds;
        var expirationTime = lockDuration - TimeSpan.FromSeconds(thresholdInSeconds);
        
        return expirationTime > TimeSpan.Zero 
            ? expirationTime 
            : null;
    }
}