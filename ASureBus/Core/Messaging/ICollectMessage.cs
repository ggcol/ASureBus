using System.Collections.Concurrent;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Messaging;

internal interface ICollectMessage
{
    internal ConcurrentQueue<IAsbMessage> Messages { get; }
}