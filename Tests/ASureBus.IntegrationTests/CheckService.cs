namespace ASureBus.IntegrationTests;

public class CheckService
{
    private readonly object _lock = new();
    
    public bool Acknowledged { get; set; }
    public double ProcessingTimeSeconds { get; private set; }
    public int ProcessedMessageCount { get; private set; }
    
    public void Acknowledge(double processingTimeSeconds = 0)
    {
        lock (_lock)
        {
            Acknowledged = true;
            ProcessingTimeSeconds = processingTimeSeconds;
        }
    }
    
    public void IncrementProcessedMessageCount()
    {
        lock (_lock)
        {
            ProcessedMessageCount++;
        }
    }
    
    public void Reset()
    {
        lock (_lock)
        {
            Acknowledged = false;
            ProcessingTimeSeconds = 0;
            ProcessedMessageCount = 0;
        }
    }
}