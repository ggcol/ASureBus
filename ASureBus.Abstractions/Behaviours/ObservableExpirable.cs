using System.Text.Json.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ASureBus.Abstractions.Behaviours;

public abstract class ObservableExpirable
{
    private readonly TimeSpan? _expiresAfter;
    private Timer? _timer;

    internal event EventHandler? Expired;
    
    [JsonIgnore]
    internal bool HasExpiration => _expiresAfter is not null;
    [JsonIgnore]
    internal DateTimeOffset? StartTime { get; }
    
    protected ObservableExpirable(TimeSpan? expiresAfter)
    {
        _expiresAfter = expiresAfter;
        if (HasExpiration) SetTimer();
    }
    
    protected ObservableExpirable(TimeSpan? expiresAfter, DateTimeOffset startTime) : this(expiresAfter)
    {
        StartTime = startTime;
    }


    private void SetTimer()
    {
        _timer = new Timer(_expiresAfter!.Value.TotalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Expired?.Invoke(this, EventArgs.Empty);
        _timer?.Dispose();
    }
}