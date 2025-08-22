namespace ASureBus.ConfigurationObjects.Options;

public sealed class MessageLockRenewalOptions 
{
    public bool? EnableMessageLockAutoRenewal { get; set; }
    public int MessageLockRenewalPreemptiveThresholdInSeconds { get; set; } =
        Defaults.ServiceBus.MESSAGE_LOCK_RENEWAL_PREEMPTIVE_THRESHOLD_IN_SECONDS;
}