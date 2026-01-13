using ASureBus.Core.Behaviours;
using ASureBus.Core.MessageProcessing.LockHandling.Entities;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.MessageProcessing.LockHandling;

internal sealed class MessageLockObserver : Observer<MessageLockObservable>, IMessageLockObserver
{
    private static readonly TimeSpan DefaultLockRenewalDuration = TimeSpan.FromSeconds(30);
    
    public void RenewOnExpiration(ProcessMessageEventArgs args)
    {
        var obs = new MessageLockObservable(args);
        if (!obs.HasExpiration)
        {
            return;
        }
        
        var lockDuration = ExtractLockDuration(args);
        RenewOnExpirationInternal(args, obs, lockDuration);
    }

    private static TimeSpan ExtractLockDuration(ProcessMessageEventArgs args)
    {
        var timeUntilLock = args.Message.LockedUntil - DateTimeOffset.UtcNow;
        return timeUntilLock > TimeSpan.Zero ? timeUntilLock : DefaultLockRenewalDuration;
    }

    private void RenewOnExpirationInternal(ProcessMessageEventArgs args, MessageLockObservable obs, TimeSpan lockDuration)
    {
        if (!obs.HasExpiration)
        {
            return;
        }

        Observe(args.Message, obs, async void (_, _) =>
        {
            if (obs.StartTime is not null)
            {
                var elapsedTime = DateTimeOffset.UtcNow - obs.StartTime.Value;

                var remainingTime = AsbConfiguration.MessageLockOptions.MaxAutoLockRenewalDuration - elapsedTime;
                if (remainingTime < lockDuration)
                {
                    return;
                }

                if (elapsedTime >= AsbConfiguration.MessageLockOptions.MaxAutoLockRenewalDuration)
                {
                    return;
                }
            }

            await args.RenewMessageLockAsync(args.Message).ConfigureAwait(false);

            var startTime = obs.StartTime ?? DateTimeOffset.UtcNow;
            var newObs = new MessageLockObservable(lockDuration, startTime);

            if (!newObs.HasExpiration)
            {
                return;
            }

            RenewOnExpirationInternal(args, newObs, lockDuration);
        });
    }
}