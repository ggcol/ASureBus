using System.Timers;
using Timer = System.Timers.Timer;

namespace ASureBus.Core.Entities;

internal abstract class ObservableExpirable
{
    internal event EventHandler? Expired;
    private readonly TimeSpan? _expiresAfter;
    private Timer? _timer;

    protected ObservableExpirable(TimeSpan? expiresAfter)
    {
        _expiresAfter = expiresAfter;
        if (_expiresAfter is not null)
            SetTimer();
    }

    internal bool HasExpiration => _expiresAfter is not null;

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