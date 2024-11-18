﻿using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Messaging;

internal abstract class CollectMessage : ICollectMessage
{
    public Queue<IAsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    protected async Task Enqueue<TMessage>(TMessage message, EmitOptions? options, 
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        await Enqueue(message, null, options, cancellationToken).ConfigureAwait(false);
    }

    protected async Task Enqueue<TMessage>(TMessage message, DateTimeOffset? scheduledTime, 
        EmitOptions? options, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();
        
        var heaviesRef = await UnloadHeavies(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        var asbMessage = ToInternalMessage(message, messageId, options, heaviesRef, scheduledTime);
        Messages.Enqueue(asbMessage);
    }

    private static async Task<IReadOnlyList<HeavyReference>> UnloadHeavies<TMessage>(
        TMessage message, Guid messageId, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        return HeavyIo.IsHeavyConfigured() 
            ? await HeavyIo.Unload(message, messageId, cancellationToken).ConfigureAwait(false) 
            : Array.Empty<HeavyReference>();
    }

    private AsbMessage<TMessage> ToInternalMessage<TMessage>(TMessage message, Guid messageId, 
        EmitOptions? options, IReadOnlyList<HeavyReference>? heavies, DateTimeOffset? scheduledTime)
        where TMessage : IAmAMessage
    {
        var asbMessage = new AsbMessage<TMessage>
        {
            MessageId = messageId,
            MessageName = typeof(TMessage).Name,
            Destination = UseDefaultDestination(options) 
                ? typeof(TMessage).Name 
                : options!.Destination,
            Message = message,
            CorrelationId = CorrelationId,
            Heavies = UseHeavies(heavies) 
                ? heavies 
                : null,
            ScheduledTime = scheduledTime
        };

        return asbMessage;
    }

    private static bool UseDefaultDestination(EmitOptions? options)
    {
        return options is null || string.IsNullOrWhiteSpace(options.Destination);
    }

    private static bool UseHeavies(IReadOnlyList<HeavyReference>? heavies)
    {
        return heavies is not null && heavies.Count > 0;
    }
}